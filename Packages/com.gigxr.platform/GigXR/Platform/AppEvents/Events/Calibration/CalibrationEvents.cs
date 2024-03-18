namespace GIGXR.Platform.AppEvents.Events.Calibration
{
    using GIGXR.Platform.Core.EventBus;

    /// <summary>
    /// Base AppEvent for anything related to Calibration.
    /// </summary>
    public abstract class BaseCalibrationEvent : IGigEvent<AppEventBus>
    {
    }
}