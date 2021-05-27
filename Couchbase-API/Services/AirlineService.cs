using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase_API.Entities;
using Couchbase_API.Repositories;

namespace Couchbase_API.Services
{
    public class AirlineService : IAirlineService
    {
        private readonly ITravelRepository _travelRepository;

        public AirlineService(ITravelRepository travelRepository)
        {
            _travelRepository = travelRepository;
        }

        public List<Airline> GetAllVersions(string id)
        {
            var airlines = _travelRepository.GetAll(id);
            return airlines;
        }

        public Airline GetLatest(string key)
        {
            var latestVersion = _travelRepository.Get(key);
            return latestVersion.Value;
        }

        public Airline UpdateWithOptimisticLock(string key, Airline airline)
        {
            var operationResult = _travelRepository.Update(key, airline);
            return operationResult.Value;
        }

        public Airline InsertNewVersionWithOptimisticLock(string key, Airline airline)
        {
            var operationResult = _travelRepository.UpsertWithVersionAndUseOptimisticLock(key, airline);
            return operationResult.Content;
        }

        public ulong LockObject(string key, TimeSpan timeSpan)
        {
            var lockedObject = _travelRepository.GetAndLock(key, timeSpan);
            return lockedObject.Cas;
        }
    }
}