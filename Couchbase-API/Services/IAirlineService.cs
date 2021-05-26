using System;
using System.Collections.Generic;
using Couchbase_API.Entities;

namespace Couchbase_API.Services
{
    public interface IAirlineService
    {
        List<Airline> GetAllVersions(string key);
        Airline GetLatest(string key);
        Airline UpdateWithOptimisticLock(string key, Airline airline);
        Airline InsertNewVersionWithOptimisticLock(string key, Airline airline);
        ulong LockObject(string key, TimeSpan timeSpan);
    }
}