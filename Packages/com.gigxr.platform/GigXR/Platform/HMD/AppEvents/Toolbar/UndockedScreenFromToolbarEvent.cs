namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;

    public class UndockedScreenFromToolbarEvent : BaseToolbarEvent
    {
        public BaseScreenObject.ScreenType UndockedScreen;

        public UndockedScreenFromToolbarEvent(BaseScreenObject.ScreenType screen) : base()
        {
            UndockedScreen = screen;
        }
    }
}