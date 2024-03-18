using GIGXR.Platform.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public static class ObjectExtensions
    {
        public static IEnumerable<MethodInfo> GetInjectableDependencies(this object obj)
        {
            return obj.GetType()
                      .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                      .Where(methodInfo => methodInfo.GetCustomAttributes<InjectDependencies>(true).Any());
        }

        public static T ImplementsAttribute<T>(this object obj) where T : Attribute
        {
            return obj.GetType().GetCustomAttribute<T>();
        }
    }

    public static class UnityObjectExtensions
    {
        public static FieldInfo[] GetVisibleSerializedFields(this UnityEngine.Object obj)
        {
            var infoFields = new List<FieldInfo>();

            var publicFields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < publicFields.Length; i++)
            {
                if (publicFields[i].GetCustomAttribute<HideInInspector>() == null)
                {
                    infoFields.Add(publicFields[i]);
                }
            }

            var privateFields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            for (int i = 0; i < privateFields.Length; i++)
            {
                if (privateFields[i].GetCustomAttribute<SerializeField>() != null)
                {
                    infoFields.Add(privateFields[i]);
                }
            }

            return infoFields.ToArray();
        }
    }
}