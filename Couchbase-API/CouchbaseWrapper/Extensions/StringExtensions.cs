namespace Couchbase_API.CouchbaseWrapper.Extensions
{
    public static class StringExtensions
    {
        public static string AppendSuffix(this string value, string suffix)
        {
            return value + suffix;
        }
    }
}