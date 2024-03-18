/*
 * THIS CLASS IS NOT IN USE.
 * 
 * It is a part of old download management utilities, and has been left
 * as a reference for upcoming Content Management efforts - CU-1x0q7ce
 */

/*
using System;
using GIGXR.GMS.Models;
using UnityEngine;

namespace GIGXR.Platform.Downloads
{
    /// <summary>
    /// A cache to store resource content lengths.
    /// </summary>
    /// <remarks>
    /// Currently the values are stored in PlayerPrefs forever and never evicted.
    /// </remarks>
    public class ResourceContentLengthCache
    {
        public static ResourceContentLengthCache Instance { get; } = new ResourceContentLengthCache();
        private const string PlayerPrefsKeyPrefix = "gigxr-resource-content-length-cache";

        public bool TryGetValue(Guid resourceId, out long contentLength)
        {
            try
            {
                var key = CreatePlayerPrefsKey(resourceId);

                if (PlayerPrefs.HasKey(key))
                {
                    contentLength = long.Parse(PlayerPrefs.GetString(key));
                    return true;
                }

                contentLength = -1;
                return false;
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                contentLength = -1;
                return false;
            }
        }

        public bool TryGetValue(Resource resource, out long contentLength) =>
            TryGetValue(resource.ResourceId, out contentLength);

        public bool TryAdd(Guid resourceId, long contentLength)
        {
            try
            {
                var key = CreatePlayerPrefsKey(resourceId);
                PlayerPrefs.SetString(key, contentLength.ToString());
                return true;
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                return false;
            }
        }

        public bool TryAdd(Resource resource, long contentLength) => TryAdd(resource.ResourceId, contentLength);

        private string CreatePlayerPrefsKey(Guid resourceId)
        {
            var key = string.Concat(PlayerPrefsKeyPrefix, "-", resourceId);
            return key;
        }
    }
}
*/
