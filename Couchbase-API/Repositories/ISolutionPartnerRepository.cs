using Couchbase_API.CouchbaseWrapper.Repositories;
using Couchbase_API.Entities;
using Couchbase.Extensions.DependencyInjection;

namespace Couchbase_API.Repositories
{
    public interface ISolutionPartnerRepository : ICouchbaseRepository<SolutionPartner>
    {
    }
}