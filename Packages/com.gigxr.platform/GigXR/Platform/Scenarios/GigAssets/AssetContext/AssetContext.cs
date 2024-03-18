using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// <c>AssetContext</c> provides key-value storage for any data that
    /// an asset might need outside of <c>GIGXR.Platform.Scenarios</c>
    /// </summary>
    public class AssetContext : IAssetContext
    {
        #region IAssetContext Implementation

        public void SetContext<T>(string name, T value)
        {
            MapContext(name, new AssetContextValue<T>(value));
        }

        public void SetContext<T>(string name, Func<T> valueSource)
        {
            MapContext(name, new AssetContextValueSource<T>(valueSource));
        }

        public T GetContext<T>(string name)
        {
            return GetContext<T>(name, default(T));
        }

        public T GetContext<T>(string name, T defaultValue)
        {
            if (contextMap.TryGetValue(name, out object value))
            {
                // try to get the value associated with the input string.
                // if conversion to the specified type fails, log the error and default.
                try
                {
                    return ((IAssetContextValue<T>)value).Value;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Asset context property \"{name}\" was found, but type {typeof(T)} could not be derived from {value.GetType()}. See exception below. Default returned.");
                    Debug.LogException(ex);
                    return defaultValue;
                }
            }
            else
            {
                // if the input string was not found, return the default
                return defaultValue;
            }
        }

        public void RemoveContext(string name)
        {
            if (!contextMap.Remove(name))
            {
                Debug.LogWarning($"Attempted to remove asset context property \"{name}\", but it was not found.");
            }
        }

        #endregion

        #region Private

        private Dictionary<string, object> contextMap = new Dictionary<string, object>();
        private void MapContext(string key, object value)
        {
            if (contextMap.ContainsKey(key))
            {
                contextMap[key] = value;
            }
            else
            {
                contextMap.Add(key, value);
            }
        }

        #endregion
    }

    /// <summary>
    /// Wrap T to fit into an IAssetContextValue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class AssetContextValue<T> : IAssetContextValue<T>
    {
        public T Value { get; }

        public AssetContextValue(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Wrap Func<T> to fit into an IAssetContextValue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class AssetContextValueSource<T> : IAssetContextValue<T>
    {
        public T Value
        {
            get
            {
                return sourceFunction.Invoke();
            }
        }

        private Func<T> sourceFunction;

        public AssetContextValueSource(Func<T> sourceFunction)
        {
            this.sourceFunction = sourceFunction;
        }
    }
}
