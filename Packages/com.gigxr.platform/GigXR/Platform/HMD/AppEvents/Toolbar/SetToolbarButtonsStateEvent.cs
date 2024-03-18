using GIGXR.Platform.HMD.UI;

namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    /// <summary>
    /// Contains the information needed to connect a screen to become a new button on the toolbar.
    /// </summary>
    public class SetToolbarButtonsStateEvent : BaseToolbarEvent
    {
        public bool State { get; }

        public SetToolbarButtonsStateEvent(bool state)
        {
            State = state;
        }
    }
}