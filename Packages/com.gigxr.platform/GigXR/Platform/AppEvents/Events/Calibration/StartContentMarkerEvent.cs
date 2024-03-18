namespace GIGXR.Platform.AppEvents.Events.Calibration
{
    /// <summary>
    /// Event sent out to trigger syncing the content marker.
    /// </summary>
    public class StartContentMarkerEvent : BaseCalibrationEvent
    {
        public bool WithAssetsHidden;

        public StartContentMarkerEvent(bool withAssetsHidden)
        {
            WithAssetsHidden = withAssetsHidden;
        }
    }
}