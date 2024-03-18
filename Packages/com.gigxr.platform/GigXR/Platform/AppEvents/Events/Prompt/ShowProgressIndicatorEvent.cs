using System.Threading;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    /// <summary>
    /// Generic event to show the progress indicator.
    /// Publishing will have no effect if the progress
    /// indicator is already showing.
    /// </summary>
    public class ShowProgressIndicatorEvent : BasePromptEvent
    {
    }

    /// <summary>
    /// Generic event to hide the progress indicator.
    /// Publishing will have no effect if the progress
    /// indicator is already hidden.
    /// </summary>
    public class HideProgressIndicatorEvent : BasePromptEvent
    {
    }
}