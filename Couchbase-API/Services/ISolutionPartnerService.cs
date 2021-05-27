using System.Collections.Generic;
using Couchbase_API.CouchbaseWrapper.Models;
using Couchbase_API.Entities;

namespace Couchbase_API.Services
{
    public interface ISolutionPartnerService
    {
        SolutionPartner Create(string key, SolutionPartner solutionPartner);
        SolutionPartner Update(string key, SolutionPartner solutionPartner);
        List<SolutionPartner> GetAll();
        List<SolutionPartner> GetAll(string key);
        SolutionPartner Get(string key);
        SolutionPartner Get(string key, ulong version);
        List<ObjectDiff> GetDifference(string key, ulong version);
    }
}