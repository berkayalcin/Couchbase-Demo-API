using System;
using System.Collections.Generic;
using Couchbase_API.CouchbaseWrapper.Models;

namespace Couchbase_API.CouchbaseWrapper.Extensions
{
    public static class ObjectExtensions
    {
        public static IEnumerable<ObjectDiff> GetDifference<T>(this T left, T right)
        {
            if (left == null || right == null)
                return null;
            var objectDiffs = new List<ObjectDiff>();
            var objectType = typeof(T);
            foreach (var propertyInfo in objectType.GetProperties())
            {
                var leftValue = propertyInfo.GetValue(left, null);
                var rightValue = propertyInfo.GetValue(right, null);
                if (!Equals(leftValue, rightValue))
                {
                    objectDiffs.Add(new ObjectDiff
                    {
                        Key = propertyInfo.Name,
                        Left = leftValue,
                        Right = rightValue
                    });
                }
            }

            return objectDiffs;
        }
    }
}