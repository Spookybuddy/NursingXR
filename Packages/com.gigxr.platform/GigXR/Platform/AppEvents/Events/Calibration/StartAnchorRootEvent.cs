using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.Calibration
{
    /// <summary>
    /// Event sent out to trigger syncing the content marker.
    /// </summary>
    public class StartAnchorRootEvent : BaseCalibrationEvent
    {
        public bool FromReset { get; }

        public StartAnchorRootEvent(bool fromReset)
        {
            FromReset = fromReset;
        }
    }
}