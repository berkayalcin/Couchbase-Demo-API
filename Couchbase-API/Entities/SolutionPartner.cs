using System.Collections.Generic;
using System.Linq;
using Couchbase_API.CouchbaseWrapper.Models;

namespace Couchbase_API.Entities
{
    public class SolutionPartner : DocumentBase<SolutionPartner>
    {
        public string Name { get; set; }
        public string LogoPath { get; set; }
        public string PhoneNumber { get; set; }
        public string WorkingHoursStart { get; set; }
        public string WorkingHoursEnd { get; set; }
        public string WorkingDays { get; set; }
        public bool IsOfferEnabled { get; set; }
        public bool IsCallEnabled { get; set; }
    }
}