using Newtonsoft.Json;

namespace Couchbase_Lock_Demo
{
    public class Airline
    {
        [JsonProperty("callsign")]
        public string CallSign { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("iata")]
        public string Iata { get; set; }

        [JsonProperty("icao")]
        public string Icao { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}