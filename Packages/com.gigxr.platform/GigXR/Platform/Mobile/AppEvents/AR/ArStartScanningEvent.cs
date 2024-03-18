using System;

namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     An event used as a command to start the scanning process
    ///     by which mobile users set their calibrated origin.
    ///     This event should only be published when AR utilities are
    ///     not enabled; if scanning needs to be started while already
    ///     using AR utilities, <see cref="ArSessionResetEvent"/> should
    ///     be published first.
    /// </summary>
    public class ArStartScanningEvent : BaseArEvent
    {
        public ArStartScanningEvent()
        {
        }
    }
}
