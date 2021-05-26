using System;
using System.Collections.Generic;
using Couchbase;
using Couchbase_API.CouchbaseWrapper.Models;
using Couchbase.Core;
using Couchbase.N1QL;

namespace Couchbase_API.CouchbaseWrapper.Repositories
{
    public interface ICouchbaseRepository<T>
        where T : DocumentBase<T>
    {
        IBucket GetBucket();
        void Unlock(string key, ulong cas);

        IOperationResult<T> GetAndLock(string key, TimeSpan? expiration = null);
        IOperationResult<T> Get(string key);
        IOperationResult<T> Get(string key, ulong version);
        IQueryResult<T> GetAll(long offset = 0, long limit = 10);
        IQueryResult<T> Query(IQueryRequest queryRequest);
        ulong GetNumeric(string key);
        Dictionary<string, T> GetAllVersions(string key);
        IDocumentResult<T> Upsert(string key, T data);
        IDocumentResult<T> VersionedUpsert(string key, T data);
        IDocumentResult<T> VersionedUpsertWithOptimisticLock(string key, T data);
        IOperationResult<T> UpsertWithOptimisticLock(string key, T data, TimeSpan? timeout = null);
        IOperationResult<T> Insert(string key, T data);
        IOperationResult<T> Update(string key, T data);
    }
}