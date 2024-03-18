/*
 * THIS CLASS IS NOT IN USE.
 * 
 * It is a part of old download management utilities, and has been left
 * as a reference for upcoming Content Management efforts - CU-1x0q7ce
 */

/*
using System;
using UnityEngine;
using GIGXR.Platform.Utilities;

namespace GIGXR.Platform.Downloads
{
    /// <summary>
    /// A place to store and retrieve the maximum content storage value. This is the maximum amount of storage allowed
    /// for downloads.
    /// </summary>
    public class MaximumContentStorage
    {
        public static MaximumContentStorage Instance { get; } = new MaximumContentStorage();
        public const int DefaultValue = 4;

        private const string PlayerPrefsKey = "gigxr-maximum-content-storage";

        public bool TryGetValueGigabytes(out int gigabytes)
        {
            gigabytes = DefaultValue;

            try
            {
                if (PlayerPrefs.HasKey(PlayerPrefsKey))
                {
                    gigabytes = PlayerPrefs.GetInt(PlayerPrefsKey);
                }

                return true;
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);  
                return false;
            }
        }

        public bool TryGetValueBytes(out long bytes)
        {
            var success = TryGetValueGigabytes(out var gigabytes);
            bytes = NumberUtils.GigabytesToBytes(gigabytes);

            return success;
        }

        public bool TrySet(int gigabytes)
        {
            try
            {
                PlayerPrefs.SetInt(PlayerPrefsKey, gigabytes);
                return true;
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                return false;
            }
        }
    }
}
*/
