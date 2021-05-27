namespace Couchbase_API.CouchbaseWrapper.Models
{
    public class ObjectDiff
    {
        public string Key { get; set; }
        public object Left { get; set; }
        public object Right { get; set; }
    }
}