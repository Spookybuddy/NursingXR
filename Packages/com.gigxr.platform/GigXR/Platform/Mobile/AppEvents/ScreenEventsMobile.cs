using GIGXR.Platform.Core.EventBus;

namespace GIGXR.Platform.Mobile.AppEvents.Events.UI
{
    using GIGXR.Platform.UI;
    using Platform.Mobile.UI;

    public abstract class BaseScreenCommandEventMobile : IGigEvent<UiEventBus>
    {
        public BaseScreenObjectMobile.ScreenTypeMobile TargetScreen;

        protected BaseScreenCommandEventMobile
        (
            BaseScreenObjectMobile.ScreenTypeMobile targetScreen
        )
        {
            TargetScreen = targetScreen;
        }
    }

    public class SwitchingActiveScreenEventMobile : BaseScreenCommandEventMobile
    {
        public readonly BaseScreenObjectMobile.ScreenTypeMobile SenderScreen;

        public SwitchingActiveScreenEventMobile
        (
            BaseScreenObjectMobile.ScreenTypeMobile targetScreen,
            BaseScreenObjectMobile.ScreenTypeMobile senderScreen
        ) : base(targetScreen)
        {
            TargetScreen = targetScreen;
            SenderScreen = senderScreen;
        }
    }
}
