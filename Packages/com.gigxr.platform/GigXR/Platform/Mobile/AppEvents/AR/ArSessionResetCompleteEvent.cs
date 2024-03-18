namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     Published at the end of processing any published
    ///     <see cref="ArSessionResetEvent"/> to inform subscribers
    ///     that the reset is complete and AR utilities can be used again
    ///     if desired. Leads to the publication of a 
    ///     <see cref="ArStartScanningEvent"/> when resetting the scan via
    ///     <see cref="Mobile.UI.ResetScanScreen.ResetScan"/>
    /// </summary>
    public class ArSessionResetCompleteEvent : BaseArEvent
    {
        public ArSessionResetCompleteEvent()
        {
        }
    }
}
