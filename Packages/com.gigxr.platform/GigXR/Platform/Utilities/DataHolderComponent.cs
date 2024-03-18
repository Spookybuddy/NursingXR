using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// A component who's responsibility is holding to hold a reference
    /// to some data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataHolderComponent : MonoBehaviour
    {
        public object data;

        public T GetData<T>()
        {
            return (T)data;
        }
    }
}