namespace GIGXR.Platform.Core.Settings
{
    using System;

    [Serializable]
    public class NetworkingProfile
    {
        /// <summary>
        /// Returns the default room name for networking. Can optionally prefix it with "[username]'s".
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetDefaultRoomName(string userName = null)
        {
            return userName == null ? DefaultRoomName : string.Concat(userName, "'s ", DefaultRoomName);
        }

        /// <summary>
        /// Defines the default room name. Will be prefixed with \"[username]'s\" if a username is input to the return method.
        /// </summary>
        public string DefaultRoomName = "Room";

        /// <summary>
        /// Returns the ping interval in seconds.
        /// </summary>
        public int PingInterval = 10;

        /// <summary>
        /// Returns the bad ping threshold in milliseconds.
        /// </summary>
        public int BadPingThreshold = 150;

        /// <summary>
        /// Returns the severe ping threshold in milliseconds.
        /// </summary>
        public int SeverePingThreshold = 250;
    }
}