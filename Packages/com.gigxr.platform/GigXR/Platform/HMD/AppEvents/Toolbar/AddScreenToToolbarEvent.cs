using GIGXR.Platform.HMD.UI;

namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    /// <summary>
    /// Contains the information needed to connect a screen to become a new button on the toolbar.
    /// </summary>
    public class AddScreenToToolbarEvent : BaseToolbarEvent
    {
        public ToolbarButtonScriptableObject ToolbarButtonInformation { get; }

        public BaseScreenObject.ScreenType ScreenType { get; }

        public AddScreenToToolbarEvent(BaseScreenObject.ScreenType screenType, ToolbarButtonScriptableObject buttonInformation)
        {
            ScreenType = screenType;
            ToolbarButtonInformation = buttonInformation;
        }
    }
}