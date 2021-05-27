using Couchbase_API.BucketProviders;
using Couchbase_API.CouchbaseWrapper.Repositories;
using Couchbase_API.Entities;
using Microsoft.Extensions.Logging;

namespace Couchbase_API.Repositories
{
    class SolutionPartnerRepository : CouchbaseRepository<SolutionPartner>, ISolutionPartnerRepository
    {
        public SolutionPartnerRepository(ISolutionPartnerBucketProvider bucketProvider,
            ILogger<ICouchbaseRepository<SolutionPartner>> logger) : base(bucketProvider, logger)
        {
        }
    }
}