using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public class CollectionUtils
    {
        public static bool CollectionNullEmptyOrContains<T>(ICollection<T> collection, T value)
        {
            return collection == null || collection.Count == 0 || collection.Contains(value);
        }
    }
}
