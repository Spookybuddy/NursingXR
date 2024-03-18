using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Helper component that can be added to a GameObject to save it's current layers in the hierarchy
    /// as well as restore them when complete.
    /// </summary>
    public class LayerCache : MonoBehaviour
    {
        private Dictionary<GameObject, int> layerCache = new Dictionary<GameObject, int>();

        private Dictionary<GameObject, int> additionalCache = new Dictionary<GameObject, int>();

        public void SetCache(string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);

            if (gameObject.layer != layer)
            {
                gameObject.SetLayerRecursively(layer, out var cache);

                if(gameObject.transform.parent != null)
                {
                    // Check to see if there is a parent cache or asset mediator as the restore function will be called on them, so GOs
                    // that end up calling this need to pass their cache data to the parent instead.
                    var parentCache = gameObject.transform.parent.gameObject.GetComponentInParent<LayerCache>();

                    // GetComponentInParent will always grab 'this' one as well, so as long as there are two or more we know there's a parent
                    if (parentCache != null && parentCache != this)
                    {
                        parentCache.AddCache(cache);

                        // The parent cache will restore this GO
                        Destroy(this);
                    }
                    else
                    {
                        SetChildCache(cache);
                    }
                }
                else
                {
                    SetChildCache(cache);
                }
            }
        }

        private void SetChildCache(Dictionary<GameObject, int> cache)
        {
            layerCache = cache;

            // It's also possible for a child of an asset to be instantiated and hidden first,
            // so check if there's a child layer cache as well
            var childCache = GetComponentInChildren<LayerCache>();

            if (childCache != null &&
                childCache != this)
            {
                AddCache(childCache.layerCache);

                Destroy(childCache);
            }
        }

        public void Restore()
        {
            // Note: We use new Dictionary here because ApplyLayer will delete the Cached as it's applied and we want to preserve the reference
            gameObject.ApplyLayerCacheRecursively(layerCache.Concat(additionalCache).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            // Destroy the component now that we are done with it, if the component is not attached, then there is no cache
            Destroy(this);
        }

        public void AddCache(Dictionary<GameObject, int> newCacheData)
        {
            if (newCacheData != null)
            {
                foreach (var newData in newCacheData)
                {
                    if (!additionalCache.ContainsKey(newData.Key))
                    {
                        additionalCache.Add(newData.Key, newData.Value);
                    }
                    else
                    {
                        additionalCache[newData.Key] = newData.Value;
                    }
                }
            }
        }
    }
}