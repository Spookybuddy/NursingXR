namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     Event used as a command to reset AR utilities. Published
    ///     when a scan-in-progress is reset via
    ///     <see cref="Mobile.UI.ResetScanScreen.ResetScan"/>,
    ///     when scanning is restarted in-session via 
    ///     <see cref="Mobile.UI.SessionScreen.Rescan"/>,
    ///     and whenever a session is left or
    ///     an attempt to join a session fails.
    ///     <seealso cref="Networking.EventBus.Events.Matchmaking.LeftRoomNetworkEvent"/>
    ///     <seealso cref="Networking.EventBus.Events.Matchmaking.JoinRoomFailedNetworkEvent"/>
    ///     <seealso cref="Networking.EventBus.Events.Connection.DisconnectedNetworkEvent"/>
    ///     When this event is published, it starts the following sequence
    ///     of events:
    ///     <list type="number">
    ///         <item> <see cref="ArSessionResetStartingEvent"/> is published,
    ///             to notify AR-dependent utilities that they should stop. 
    ///             </item>
    ///         <item> Plane detection is disabled. </item>
    ///         <item> The AR session is disabled. </item>
    ///         <item> Plane detection is reset. </item>
    ///         <item> <see cref="ArSessionResetStartingEvent"/> is published, to
    ///             notify any subscribed utilities that the reset is complete.
    ///             </item>
    ///     </list>
    /// </summary>
    public class ArSessionResetEvent : BaseArEvent
    {
        public ArSessionResetEvent()
        {
        }
    }
}
