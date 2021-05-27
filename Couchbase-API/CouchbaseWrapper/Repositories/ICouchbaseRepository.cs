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
        List<T> GetAll(string key, long offset = 0, long limit = 10);
        IQueryResult<T> Query(IQueryRequest queryRequest);
        IDocumentResult<T> Upsert(string key, T data);
        IDocumentResult<T> UpsertWithVersion(string key, T data);
        IDocumentResult<T> UpsertWithVersionAndUseOptimisticLock(string key, T data);
        IOperationResult<T> UpsertAndUseOptimisticLock(string key, T data, TimeSpan? timeout = null);
        IDocumentResult<T> Insert(string key, T data);
        IOperationResult<T> Update(string key, T data);
        IDocumentResult<T> InsertWithVersion(string key, T data);
        List<ObjectDiff> GetDifference(string key, ulong versionNumber);
    }
}