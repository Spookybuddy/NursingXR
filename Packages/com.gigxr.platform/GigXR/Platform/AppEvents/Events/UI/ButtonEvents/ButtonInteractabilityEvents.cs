namespace GIGXR.Platform.AppEvents.Events.UI.ButtonEvents
{
    using GIGXR.Platform.Core.EventBus;
    using GIGXR.Platform.UI;

    /// <summary>
    /// Base event for a collection of button interactability events.
    /// </summary>
    public abstract class BaseButtonRequestEvent : IGigEvent<UiEventBus>
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class SettingGlobalButtonStateEvent : BaseButtonRequestEvent
    {
        public readonly bool setActive;
        public readonly bool fadeButton; 
        
        public SettingGlobalButtonStateEvent
        (
            bool setActive,
            bool fadeButton
        )
        {
            this.setActive = setActive;
            this.fadeButton = fadeButton;
        }
    }
}
