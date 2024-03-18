namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    /// <summary>
    /// Activate or deactivate the toolbar with this event.
    /// </summary>
    public class SetToolbarStateEvent : BaseToolbarEvent
    {
        public bool ToolbarState { get; }

        public SetToolbarStateEvent(bool shouldEnable)
        {
            ToolbarState = shouldEnable;
        }
    }
}