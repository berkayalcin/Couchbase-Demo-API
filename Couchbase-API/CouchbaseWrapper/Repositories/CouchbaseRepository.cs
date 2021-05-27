using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Couchbase_API.CouchbaseWrapper.Extensions;
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
        private const string StartsWithSuffix = "%";
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

        public List<T> GetAll(string key, long offset = 0, long limit = 10)
        {
            key = key.AppendSuffix(StartsWithSuffix);
            var queryRequest = new QueryRequest()
                .Statement(
                    $"SELECT `{_bucketName}`.* FROM `{_bucketName}` WHERE META(`{_bucketName}`).id LIKE $1 OFFSET {offset} LIMIT {limit}")
                .AddPositionalParameter(key);
            var queryResult = _bucket.Query<T>(queryRequest);
            return queryResult.Rows?
                .Where(r => r.Version > 0)
                .GroupBy(g => g.Version)
                .Select(gr => gr.First())
                .ToList();
        }

        public List<ObjectDiff> GetDifference(string key, ulong versionNumber)
        {
            var currentVersion = _bucket.Get<T>(key);
            var currentData = currentVersion.Value;
            var versionToCompare = Get(key, versionNumber);
            var dataToCompare = versionToCompare.Value;

            var differences = currentData.GetDifference(dataToCompare);
            return differences.ToList();
        }

        public IQueryResult<T> Query(IQueryRequest queryRequest)
        {
            var queryResult = _bucket.Query<T>(queryRequest);
            return queryResult;
        }


        public IDocumentResult<T> Insert(string key, T data)
        {
            var document = BuildDocument(data, key);
            SetAuditProperties(document.Content);
            return Upsert(key, data);
        }

        public IDocumentResult<T> InsertWithVersion(string key, T data)
        {
            var document = BuildDocument(data, key);
            SetAuditProperties(document.Content);
            return UpsertWithVersion(key, data);
        }

        public IOperationResult<T> Update(string key, T data)
        {
            SetAuditProperties(data);
            return UpsertAndUseOptimisticLock(key, data);
        }

        public IDocumentResult<T> Upsert(string key, T data)
        {
            SetAuditProperties(data);
            var document = BuildDocument(data, key);
            var result = _bucket.Upsert(document);
            return result;
        }

        public IDocumentResult<T> UpsertWithVersion(string key, T data)
        {
            var newVersionNumber = GetNewVersionNumber(key);
            SetAuditProperties(data);
            SetDocumentVersion(data, newVersionNumber);
            var newKeyWithVersionNumber = GetKeyWithVersionNumber(key, newVersionNumber);
            var latestDocument = BuildDocument(data, key);
            var newDocument = BuildDocument(data, newKeyWithVersionNumber);
            _bucket.Upsert(latestDocument);
            var result = _bucket.Upsert(newDocument);
            return result;
        }

        public IDocumentResult<T> UpsertWithVersionAndUseOptimisticLock(string key, T data)
        {
            SetAuditProperties(data);
            var newVersionNumber = GetNewVersionNumber(key);
            SetDocumentVersion(data, newVersionNumber);
            var newKeyWithVersionNumber = GetKeyWithVersionNumber(key, newVersionNumber);
            UpsertAndUseOptimisticLock(key, data);
            return Upsert(newKeyWithVersionNumber, data);
        }

        public IOperationResult<T> UpsertAndUseOptimisticLock(string key, T data, TimeSpan? timeout = null)
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

        private static Document<T> BuildDocument(T data, string key)
        {
            return new()
            {
                Id = key,
                Expiry = 0,
                Content = data
            };
        }

        private static void SetDocumentVersion(T data, ulong version)
        {
            data.Version = version;
        }

        private static void SetAuditProperties(T data)
        {
            data.Created ??= DateTime.UtcNow;
            data.Updated ??= DateTime.UtcNow;
        }

        private ulong GetNewVersionNumber(string key)
        {
            var versionKey = $"{key}{VersionSuffix}";
            var operationResult = _bucket.Increment(versionKey, 1, 1);
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
    }
}