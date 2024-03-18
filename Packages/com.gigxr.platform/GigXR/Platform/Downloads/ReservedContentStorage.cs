/*
 * THIS CLASS IS NOT IN USE.
 * 
 * It is a part of old download management utilities, and has been left
 * as a reference for upcoming Content Management efforts - CU-1x0q7ce
 */

/*
using System;
using UnityEngine;

namespace GIGXR.Platform.Downloads
{
    /// <summary>
    /// Represents a place to store and retrieve the reserved content storage value. Reserved content storage is space
    /// that is allocated to in-progress downloads.
    /// </summary>
    /// <remarks>
    /// This has its own class because the value needs to be persisted across app restarts.
    /// </remarks>
    public class ReservedContentStorage
    {
        public static ReservedContentStorage Instance { get; } = new ReservedContentStorage();

        private const string PlayerPrefsKey = "gigxr-reserved-content-storage";

        /// <summary>
        /// Gets the current reserved content storage in bytes.
        /// </summary>
        /// <param name="bytes">The reserved content storage in byte.</param>
        /// <returns>True if the operation succeeded; false otherwise.</returns>
        public bool TryGetValueBytes(out long bytes)
        {
            bytes = 0;

            try
            {
                if (PlayerPrefs.HasKey(PlayerPrefsKey))
                {
                    bytes = long.Parse(PlayerPrefs.GetString(PlayerPrefsKey));
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

        /// <summary>
        /// Add the provided number of bytes to the reserved content storage.
        ///
        /// Use a negative value to subtract.
        /// </summary>
        /// <param name="bytes">The number of bytes to add.</param>
        /// <returns>True if the operation succeeded; false otherwise.</returns>
        public bool TryAdd(long bytes)
        {
            try
            {
                TryGetValueBytes(out var reservedContentStorage);
                reservedContentStorage += bytes;
                PlayerPrefs.SetString(PlayerPrefsKey, reservedContentStorage.ToString());
                return true;
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                return false;
            }
        }

        /// <summary>
        /// Resets the reserved content storage to 0.
        /// </summary>
        /// <returns>True if the operation succeeded; false otherwise.</returns>
        public bool TryClear()
        {
            try
            {
                PlayerPrefs.SetString(PlayerPrefsKey, 0L.ToString());
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