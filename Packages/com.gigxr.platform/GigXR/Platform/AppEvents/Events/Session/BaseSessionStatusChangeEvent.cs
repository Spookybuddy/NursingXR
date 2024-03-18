using GIGXR.Platform.Core.EventBus;
using System;

namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Base event related to whenever session related data changes
    /// </summary>
    public abstract class BaseSessionStatusChangeEvent : IGigEvent<AppEventBus>
    {
    }
}