using Couchbase_API.BucketProviders;
using Couchbase_API.CouchbaseWrapper.Repositories;
using Couchbase_API.Entities;
using Microsoft.Extensions.Logging;

namespace Couchbase_API.Repositories
{
    public class TravelRepository : CouchbaseRepository<Airline>, ITravelRepository
    {
        public TravelRepository(ITravelBucketProvider travelBucketProvider, ILogger<ITravelRepository> logger) : base(
            travelBucketProvider, logger)
        {
        }
    }
}