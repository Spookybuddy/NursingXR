namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     Published at the start of processing any published
    ///     <see cref="ArSessionResetEvent"/> to allow AR-dependent utilities
    ///     to prepare for the incoming reset.
    /// </summary>
    public class ArSessionResetStartingEvent : BaseArEvent
    {
        public ArSessionResetStartingEvent()
        {
        }
    }
}