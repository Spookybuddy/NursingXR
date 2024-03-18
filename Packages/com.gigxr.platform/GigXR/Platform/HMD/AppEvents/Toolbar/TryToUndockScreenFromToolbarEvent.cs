namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;

    public class TryToUndockScreenFromToolbarEvent : BaseToolbarEvent
    {
        public BaseScreenObject.ScreenType UndockedScreen;

        public TryToUndockScreenFromToolbarEvent(BaseScreenObject.ScreenType screen) : base()
        {
            UndockedScreen = screen;
        }
    }
}