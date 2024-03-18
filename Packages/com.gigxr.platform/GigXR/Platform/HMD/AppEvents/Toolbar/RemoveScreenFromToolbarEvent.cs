using GIGXR.Platform.HMD.UI;

namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    /// <summary>
    /// Contains the information needed to connect a screen to become a new button on the toolbar.
    /// </summary>
    public class RemoveScreenFromToolbarEvent : BaseToolbarEvent
    {
        public BaseScreenObject.ScreenType ScreenType { get; }

        public RemoveScreenFromToolbarEvent(BaseScreenObject.ScreenType screenType)
        {
            ScreenType = screenType;
        }
    }
}