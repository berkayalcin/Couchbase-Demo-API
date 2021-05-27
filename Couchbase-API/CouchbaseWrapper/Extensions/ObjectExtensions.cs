using System;
using System.Collections.Generic;
using Couchbase_API.CouchbaseWrapper.Models;

namespace Couchbase_API.CouchbaseWrapper.Extensions
{
    public static class ObjectExtensions
    {
        public static IEnumerable<ObjectDiff> GetDifference<T>(this T left, T right)
        {
            var objectType = typeof(T);
            foreach (var propertyInfo in objectType.GetProperties())
            {
                var leftValue = propertyInfo.GetValue(left, null);
                var rightValue = propertyInfo.GetValue(right, null);
                if (!Equals(leftValue, rightValue))
                {
                    yield return new ObjectDiff
                    {
                        Key = propertyInfo.Name,
                        Left = leftValue,
                        Right = rightValue
                    };
                }
            }
        }
    }
}