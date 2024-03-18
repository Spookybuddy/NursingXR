using GIGXR.Platform.Core.EventBus;

namespace GIGXR.Platform.Networking.EventBus.Events
{
    /// <summary>
    /// A base event for the networking namespace.
    /// </summary>
    public abstract class BaseNetworkEvent : IGigEvent<NetworkManager> // TODO - should this be set to network manager?
    {
    }
}