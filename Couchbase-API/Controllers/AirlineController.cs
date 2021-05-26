using System;
using System.Threading;
using Couchbase_API.Entities;
using Couchbase_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Couchbase_API.Controllers
{
    [Route("v1/airlines")]
    [ApiController]
    public class AirlineController : Controller
    {
        private readonly IAirlineService _airlineService;

        public AirlineController(IAirlineService airlineService)
        {
            _airlineService = airlineService;
        }

        [HttpGet("{key}/all-versions")]
        public IActionResult GetAllVersions(string key)
        {
            var airlines = _airlineService.GetAllVersions(key);
            return Ok(airlines);
        }

        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            var airline = _airlineService.GetLatest(key);
            return Ok(airline);
        }

        [HttpPut("{key}/new-version-with-optimistic-lock")]
        public IActionResult UpdateWithOptimisticLock(string key, [FromBody] Airline airline)
        {
            var result = _airlineService.UpdateWithOptimisticLock(key, airline);
            return Ok(result);
        }

        [HttpPost("{key}/with-optimistic-lock")]
        public IActionResult InsertNewVersionWithOptimisticLock(string key, [FromBody] Airline airline)
        {
            var result = _airlineService.InsertNewVersionWithOptimisticLock(key, airline);
            return Ok(result);
        }

        [HttpPut("{key}/lock")]
        public IActionResult Lock(string key, int seconds)
        {
            var thread = new Thread(() =>
            {
                _airlineService.LockObject(key, TimeSpan.FromSeconds(seconds));
                Thread.Sleep(seconds * 1000 - 1);
            });
            thread.Start();
            thread.Join();

            return Ok();
        }
    }
}