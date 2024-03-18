using ExitGames.Client.Photon;
using GIGXR.Platform.Core.EventBus;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus
{
    /// <summary>
    /// A special type of <c>IGigEvent</c> that can be sent across the internet.
    /// </summary>
    public interface ICustomNetworkEvent : IGigEvent<NetworkManager>
    {
        /// <inheritdoc cref="Photon.Realtime.RaiseEventOptions"/>
        RaiseEventOptions RaiseEventOptions { get; }

        /// <inheritdoc cref="ExitGames.Client.Photon.SendOptions"/>
        SendOptions SendOptions { get; }
    }
}