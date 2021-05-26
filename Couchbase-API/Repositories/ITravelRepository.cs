using Couchbase_API.CouchbaseWrapper.Repositories;
using Couchbase_API.Entities;

namespace Couchbase_API.Repositories
{
    public interface ITravelRepository : ICouchbaseRepository<Airline>
    {
    }
}