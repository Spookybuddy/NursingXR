using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetAllAssignableTypes(this Type ofType)
        {
            // TODO: Ideally this indexing could be done at build-time and then provided to the runtime via code
            // generation or something like that.
            return AssemblyHelper.GetTypesInAllLoadedAssembliesSafely()
                .Where(type => ofType.IsAssignableFrom(type) && !type.IsInterface);
        }
    }
}