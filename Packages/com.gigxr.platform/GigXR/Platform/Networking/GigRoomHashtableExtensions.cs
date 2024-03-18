using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace GIGXR.Platform.Networking
{
    /// <summary>
    /// Extension methods for Photon RoomOptions/Room in order to set values in CustomRoomProperties with strong types.
    /// </summary>
    public static class GigRoomHashtableExtensions
    {
        public const string OwnerIdPropertyKey = "o";
        public const string HostIdPropertyKey = "h";
        public const string SessionPathwayPropertyKey = "p";
        public const string SessionPlayModePropertyKey = "m";
        public const string EphemeralData = "e";

        public static Guid GetOwnerId(this Hashtable hashtable)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            if (hashtable.ContainsKey(OwnerIdPropertyKey))
                return Guid.Parse((string)hashtable[OwnerIdPropertyKey]);
            else
                return Guid.Empty;
            
        }

        public static void SetOwnerId(this Hashtable hashtable, string ownerId)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            hashtable[OwnerIdPropertyKey] = ownerId;
        }

        public static void MapGigAssetIdToPhotonViewId(this Hashtable hashtable, string assetId, int photonId)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            hashtable[assetId] = photonId;
        }

        public static int GetPhotonViewIdFromAssetId(this Hashtable hashtable, string assetId)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            if (hashtable.ContainsKey(assetId))
            {
                return (int)hashtable[assetId];
            }
            else
            {
                return -1;
            }
        }

        public static void SetPlayMode(this Hashtable hashtable, Scenarios.ScenarioControlTypes scenarioPlayMode)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));
            hashtable[SessionPlayModePropertyKey] = (int)scenarioPlayMode;
        }

        public static int GetPlayMode(this Hashtable hashtable)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            if (hashtable.ContainsKey(SessionPlayModePropertyKey))
                return (int)hashtable[SessionPlayModePropertyKey];
            else
                return -1;
        }

        public static void SetScenarioPathway(this Hashtable hashtable, string pathwayJson)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));
            hashtable[SessionPathwayPropertyKey] = pathwayJson;
        }

        public static string GetScenarioPathway(this Hashtable hashtable)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            if (hashtable.ContainsKey(SessionPathwayPropertyKey))
                return (string)hashtable[SessionPathwayPropertyKey];
            else
                return null;
        }

        public static Guid GetHostId(this Hashtable hashtable)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            if (hashtable.ContainsKey(HostIdPropertyKey))
                return Guid.Parse((string)hashtable[HostIdPropertyKey]);
            else
                return Guid.Empty;
        }

        public static void SetHostId(this Hashtable hashtable, string ownerId)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            hashtable[HostIdPropertyKey] = ownerId;
        }

        public static void SetEphemeralDataId(this Hashtable hashtable, Guid ephemeralId)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            hashtable[EphemeralData] = ephemeralId;
        }

        public static Guid GetEphemeralDataId(this Hashtable hashtable)
        {
            if (hashtable == null)
                throw new ArgumentNullException(nameof(hashtable));

            if (hashtable.ContainsKey(EphemeralData))
                return (Guid)hashtable[EphemeralData];
            else
                return Guid.Empty;
        }
    }
}