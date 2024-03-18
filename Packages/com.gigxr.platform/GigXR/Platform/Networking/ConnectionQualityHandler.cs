using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Core;
using Photon.Pun;

namespace GIGXR.Platform.Networking
{
    /// <summary>
    /// Responsible for pinging the connected Photon server periodically and evaluating the connection quality.
    /// </summary>
    public class ConnectionQualityHandler : BaseBackgroundHandler
    {
        public event Action<int, PingStatus> PingUpdate;

        private readonly ProfileManager ProfileManager;
        private PingStatus pingStatus;

        protected override int MillisecondsDelay { get; }

        public ConnectionQualityHandler(ProfileManager profileManager)
        {
            ProfileManager = profileManager;

            // PingInterval = Seconds
            MillisecondsDelay = profileManager.networkProfile.PingInterval * 1000;
        }

        protected override UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            var ping = GetPing();

            if (ping > ProfileManager.networkProfile.SeverePingThreshold)
            {
                pingStatus = PingStatus.Severe;
            }
            else if (ping > ProfileManager.networkProfile.BadPingThreshold)
            {
                pingStatus = PingStatus.Bad;
            }
            else if (ping <= ProfileManager.networkProfile.BadPingThreshold)
            {
                pingStatus = PingStatus.Nominal;
            }

            PingUpdate?.Invoke(ping, pingStatus);

            return UniTask.CompletedTask;
        }

        protected virtual int GetPing()
        {
            return PhotonNetwork.GetPing();
        }
    }
}