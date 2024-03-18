namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;

    public class DockedScreenFromToolbarEvent : BaseToolbarEvent
    {
        public BaseScreenObject.ScreenType DockedScreen;

        public DockedScreenFromToolbarEvent(BaseScreenObject.ScreenType screen) : base()
        {
            DockedScreen = screen;
        }
    }
}