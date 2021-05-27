using System.Collections.Generic;
using Couchbase_API.CouchbaseWrapper.Models;
using Couchbase_API.Entities;
using Couchbase_API.Repositories;

namespace Couchbase_API.Services
{
    public class SolutionPartnerService : ISolutionPartnerService
    {
        private readonly ISolutionPartnerRepository _solutionPartnerRepository;

        public SolutionPartnerService(ISolutionPartnerRepository solutionPartnerRepository)
        {
            _solutionPartnerRepository = solutionPartnerRepository;
        }

        public SolutionPartner Create(string key, SolutionPartner solutionPartner)
        {
            var result = _solutionPartnerRepository.InsertWithVersion(key, solutionPartner);
            return result.Content;
        }

        public SolutionPartner Update(string key, SolutionPartner solutionPartner)
        {
            var result = _solutionPartnerRepository.UpsertWithVersionAndUseOptimisticLock(key, solutionPartner);
            return result.Content;
        }

        public List<SolutionPartner> GetAll()
        {
            var solutionPartners = _solutionPartnerRepository.GetAll(limit: 1000);
            return solutionPartners.Rows;
        }

        public List<SolutionPartner> GetAll(string key)
        {
            var solutionPartnerVersions = _solutionPartnerRepository.GetAll(key);
            return solutionPartnerVersions;
        }

        public SolutionPartner Get(string key)
        {
            var solutionPartner = _solutionPartnerRepository.Get(key);
            return solutionPartner.Value;
        }

        public SolutionPartner Get(string key, ulong version)
        {
            var solutionPartner = _solutionPartnerRepository.Get(key, version);
            return solutionPartner.Value;
        }

        public List<ObjectDiff> GetDifference(string key, ulong version)
        {
            var objectDiffs = _solutionPartnerRepository.GetDifference(key, version);
            return objectDiffs;
        }
    }
}