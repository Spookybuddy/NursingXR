namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.UI;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.Interfaces;
    using System.Collections.Generic;
    using UnityEngine;
    using GIGXR.Platform.Core.DependencyInjection;

    public class ToggleGroup : BaseUiObject
    {
        [SerializeField]
        private List<ButtonComponent> toggleableButtons;

        private ButtonComponent currentlySelectedButton;

        private ITabInput tabInputReceiver;

        private UiEventBus UIEventBus;

        [InjectDependencies]
        public void Construct(UiEventBus uiEventBus)
        {
            UIEventBus = uiEventBus;
        }

        /// <summary>
        /// Called when initially constructing the Session Screen object. 
        /// </summary>
        /// <param name="event"></param>
        private void OnSelectTabFromToggleGroup(SelectTabFromToggleGroup @event)
        {
            if (@event.SelectedTab >= 0 &&
                @event.SelectedTab < toggleableButtons.Count &&
                currentlySelectedButton != toggleableButtons[@event.SelectedTab] &&
                gameObject.activeInHierarchy)
            {
                var button = toggleableButtons[@event.SelectedTab];

                Button_OnClick(button);

                button.TriggerClick();
            }
        }

        private void OnSettingToggleButtonStateEvent(SettingToggleButtonStateEvent @event)
        {
            foreach (var currentButton in toggleableButtons)
            {
                // Disabled expects the opposite of setActive, so invert the first bool
                // For fading, it causes a 'flash' since it happens quick so don't bother
                currentButton.IsDisabled(!@event.SetActive, false);
            }
        }

        private void Button_OnClick(ButtonComponent button)
        {
            if (button == null)
            {
                Debug.Log("button is null here in Button_OnClick");
                return;
            } 
            
            currentlySelectedButton?.Highlight(false); // TODO 

            currentlySelectedButton = button;

            currentlySelectedButton.Highlight(true);

            tabInputReceiver?.TabSelected(toggleableButtons.IndexOf(currentlySelectedButton));
        }

        #region UnityFunctions

        private void Awake()
        {
            tabInputReceiver = GetComponentInParent<ITabInput>();

            foreach (var button in toggleableButtons)
            {
                button.OnClick += Button_OnClick;
            }
        }

        private void OnApplicationQuit()
        {
            foreach (var button in toggleableButtons)
            {
                button.OnClick -= Button_OnClick;
            }
        }

        private void OnDisable()
        {
            currentlySelectedButton?.Highlight(false);

            currentlySelectedButton = null;
        }

        protected override void SubscribeToEventBuses()
        {
            UIEventBus.Subscribe<SelectTabFromToggleGroup>(OnSelectTabFromToggleGroup);
            UIEventBus.Subscribe<SettingToggleButtonStateEvent>(OnSettingToggleButtonStateEvent);
        }

        #endregion
    }
}