using GIGXR.Platform.Mobile.AR;
using System;

namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     Event raised when the AR Target is instantiated during scanning.
    ///     This occurs after plane detection has found enough contiguous surface
    ///     area to use for content placement.
    /// </summary>
    public class ArTargetInstantiatedEvent : BaseArEvent
    {
        ArObject ArObject { get; }

        public ArTargetInstantiatedEvent(ArObject arObject)
        {
            ArObject = arObject;
        }
    }
}
