using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Core.ScriptableObjects
{
    public class InjectableScriptableObjectCollection : ScriptableObject, IEnumerable<ScriptableObject>
    {
        [SerializeField]
        private List<ScriptableObject> scriptableObjects;

#if UNITY_EDITOR
        private void OnValidate()
        {
            HashSet<Type> injectableTypes = new HashSet<Type>();
            foreach(var scriptableObject in scriptableObjects)
            {
                Type type = scriptableObject.GetType();
                if (injectableTypes.Contains(type))
                {
                    // dependency injection is done by type only, so there can only be one injectable of each type.
                    Debug.LogError($"{nameof(InjectableScriptableObjectCollection)} \"{name}\" contains multiple objects of type \"{type}\".");
                }
                else
                {
                    injectableTypes.Add(type);
                }
            }
        }
#endif

        #region IEnumerable
        public IEnumerator<ScriptableObject> GetEnumerator()
        {
            foreach (var so in scriptableObjects)
            {
                yield return so;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
