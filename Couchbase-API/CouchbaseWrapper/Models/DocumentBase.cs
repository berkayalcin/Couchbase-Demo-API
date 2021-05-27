using System;

namespace Couchbase_API.CouchbaseWrapper.Models
{
    public abstract class DocumentBase<T> : DocumentBase
    {
        public string Type => typeof(T).Name.ToLowerInvariant();
        public ulong Version { get; set; }
    }

    public abstract class DocumentBase
    {
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}