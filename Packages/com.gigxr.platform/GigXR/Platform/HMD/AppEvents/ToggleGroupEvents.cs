namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;
    using GIGXR.Platform.UI;

    public abstract class ToggleGroupEvent : IGigEvent<UiEventBus>
    {
        protected ToggleGroupEvent()
        {
        }
    }

    public class SelectTabFromToggleGroup : ToggleGroupEvent
    {
        // TODO Index references are fast but bad
        public int SelectedTab { get; }

        public SelectTabFromToggleGroup(int tabToSelect)
        {
            SelectedTab = tabToSelect;
        }
    }

    // Like SettingGlobalButtonStateEvent, but only for the contents of a ToggleGroup
    public class SettingToggleButtonStateEvent : ToggleGroupEvent
    {
        public bool SetActive { get; }

        public SettingToggleButtonStateEvent(bool setActive)
        {
            SetActive = setActive;
        }
    }
}