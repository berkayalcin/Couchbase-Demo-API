using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Couchbase;
using Couchbase_API.CouchbaseWrapper.Models;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.Extensions.Logging;

namespace Couchbase_API.CouchbaseWrapper.Repositories
{
    public abstract class CouchbaseRepository<T> : ICouchbaseRepository<T>
        where T : DocumentBase<T>
    {
        private readonly ILogger<ICouchbaseRepository<T>> _logger;
        private readonly IBucket _bucket;
        private const string VersionSuffix = "_version";
        private const string VersionPrefix = "::v";
        private readonly string _bucketName;

        protected CouchbaseRepository(INamedBucketProvider bucketProvider, ILogger<ICouchbaseRepository<T>> logger)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket();
            _bucketName = _bucket.Name;
        }

        public IBucket GetBucket()
        {
            return _bucket;
        }

        public void Unlock(string key, ulong cas)
        {
            var result = _bucket.Unlock(key, cas);
            if (result.Success)
                return;
            throw result.Exception;
        }

        #region Get

        public IOperationResult<T> GetAndLock(string key, TimeSpan? expiration = null)
        {
            expiration ??= TimeSpan.FromSeconds(10);
            var result = _bucket.GetAndLock<T>(key, expiration.Value);
            if (!result.Success)
            {
                throw result.Exception;
            }

            return result;
        }

        public IOperationResult<T> Get(string key)
        {
            return _bucket.Get<T>(key);
        }

        public IOperationResult<T> Get(string key, ulong version)
        {
            var keyWithVersion = GetKeyWithVersionNumber(key, version);
            return _bucket.Get<T>(keyWithVersion);
        }

        public IQueryResult<T> GetAll(long offset = 0, long limit = 10)
        {
            var statement = $"SELECT `{_bucketName}`.* from `{_bucketName}` OFFSET {offset} LIMIT {limit}";
            var queryRequest = new QueryRequest()
                .Statement(statement)
                .AddPositionalParameter(_bucketName);
            var queryResult = _bucket.Query<T>(queryRequest);
            return queryResult;
        }

        public IQueryResult<T> Query(IQueryRequest queryRequest)
        {
            var queryResult = _bucket.Query<T>(queryRequest);
            return queryResult;
        }

        public ulong GetNumeric(string key)
        {
            var document = _bucket.Get<string>(key);
            return string.IsNullOrEmpty(document.Value) ? 0 : Convert.ToUInt64(document.Value);
        }

        public Dictionary<string, T> GetAllVersions(string key)
        {
            var versions = new Dictionary<string, T>();
            var latestVersionKey = GetVersionKey(key);
            var versionsCount = GetNumeric(latestVersionKey);
            if (versionsCount == 0)
            {
                var latestVersion = _bucket.Get<T>(key);
                versions.Add(GetKeyWithVersionNumber(key, 0), latestVersion.Value);
                return versions;
            }

            for (ulong i = 1; i <= versionsCount; i++)
            {
                var versionKey = GetKeyWithVersionNumber(key, i);
                var document = _bucket.Get<T>(versionKey);
                versions.Add(versionKey, document.Value);
            }

            var lastVersion = _bucket.Get<T>(key);
            versions.Add(GetKeyWithVersionNumber(key, versionsCount + 1), lastVersion.Value);

            return versions;
        }

        #endregion

        #region Upsert

        public IOperationResult<T> Insert(string key, T data)
        {
            var document = BuildDocument(data, key);
            SetAuditProperties(document.Content);
            return UpsertWithOptimisticLock(key, data);
        }

        public IOperationResult<T> Update(string key, T data)
        {
            SetAuditProperties(data);
            return UpsertWithOptimisticLock(key, data);
        }

        public IDocumentResult<T> Upsert(string key, T data)
        {
            SetAuditProperties(data);
            var document = BuildDocument(data, key);
            var result = _bucket.Upsert(document);
            return result;
        }

        public IDocumentResult<T> VersionedUpsert(string key, T data)
        {
            SetAuditProperties(data);
            var newVersionNumber = GetNewVersionNumber(key);
            var newKeyWithVersionNumber = GetKeyWithVersionNumber(key, newVersionNumber);
            var latestDocument = BuildDocument(data, key);
            var newDocument = BuildDocument(data, newKeyWithVersionNumber);
            _bucket.Upsert(latestDocument);
            var result = _bucket.Upsert(newDocument);
            return result;
        }

        public IDocumentResult<T> VersionedUpsertWithOptimisticLock(string key, T data)
        {
            SetAuditProperties(data);
            var newVersionNumber = GetNewVersionNumber(key);
            var newKeyWithVersionNumber = GetKeyWithVersionNumber(key, newVersionNumber);
            UpsertWithOptimisticLock(key, data);
            return Upsert(newKeyWithVersionNumber, data);
        }

        public IOperationResult<T> UpsertWithOptimisticLock(string key, T data, TimeSpan? timeout = null)
        {
            SetAuditProperties(data);
            timeout ??= TimeSpan.FromSeconds(15);
            var task = Task.Run(() =>
            {
                while (true)
                {
                    var existingDocumentResult = _bucket.Get<T>(key);
                    var currentCas = existingDocumentResult.Cas;
                    var newDocumentResult = _bucket.Replace(key, data, currentCas);
                    switch (newDocumentResult.Success)
                    {
                        case true:
                            return newDocumentResult;
                        case false when newDocumentResult.Exception is CasMismatchException:
                            _logger.LogError(newDocumentResult.Exception, newDocumentResult.Message);
                            break;
                        case false:
                            throw newDocumentResult.Exception;
                    }
                }
            });
            if (task.Wait(timeout.Value))
            {
                return task.Result;
            }

            throw new Exception("Operation timed out");
        }

        #endregion


        private static Document<T> BuildDocument(T data, string key)
        {
            return new()
            {
                Id = key,
                Expiry = 0,
                Content = data
            };
        }

        private static void SetAuditProperties(T data)
        {
            data.Created ??= DateTime.UtcNow;
            data.Updated ??= DateTime.UtcNow;
        }

        private ulong GetNewVersionNumber(string key)
        {
            var operationResult = _bucket.Increment(GetVersionKey(key), 1, 1);
            if (!operationResult.Success)
            {
                throw operationResult.Exception;
            }

            return operationResult.Value;
        }

        private static string GetKeyWithVersionNumber(string key, ulong version)
        {
            return $"{key}{VersionPrefix}{version}";
        }

        private static string GetVersionKey(string key)
        {
            return $"{key}{VersionSuffix}";
        }
    }
}