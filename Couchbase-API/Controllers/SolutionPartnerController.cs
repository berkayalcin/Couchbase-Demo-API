using Couchbase_API.Entities;
using Couchbase_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Couchbase_API.Controllers
{
    [ApiController]
    [Route("v1/solution-partners")]
    public class SolutionPartnerController : Controller
    {
        private readonly ISolutionPartnerService _solutionPartnerService;

        public SolutionPartnerController(ISolutionPartnerService solutionPartnerService)
        {
            _solutionPartnerService = solutionPartnerService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var solutionPartners = _solutionPartnerService.GetAll();
            return Ok(solutionPartners);
        }

        [HttpGet("{key}/versions")]
        public IActionResult GetAll(string key)
        {
            var solutionPartnerVersions = _solutionPartnerService.GetAll(key);
            return Ok(solutionPartnerVersions);
        }

        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            var solutionPartner = _solutionPartnerService.Get(key);
            return Ok(solutionPartner);
        }

        [HttpGet("{key}/difference/{version}")]
        public IActionResult Difference(string key, ulong version)
        {
            var differences = _solutionPartnerService.GetDifference(key, version);
            return Ok(differences);
        }

        [HttpPost("{key}")]
        public IActionResult Create(string key, [FromBody] SolutionPartner solutionPartner)
        {
            var result = _solutionPartnerService.Create(key, solutionPartner);
            return Ok(result);
        }

        [HttpPut("{key}")]
        public IActionResult Update(string key, [FromBody] SolutionPartner solutionPartner)
        {
            var result = _solutionPartnerService.Update(key, solutionPartner);
            return Ok(result);
        }
    }
}