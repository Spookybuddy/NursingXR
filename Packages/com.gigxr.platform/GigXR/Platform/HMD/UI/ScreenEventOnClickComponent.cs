using GIGXR.Platform.HMD.AppEvents.Events.UI;
using GIGXR.Platform.UI;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using UnityEngine;

namespace GIGXR.Platform.HMD.UI
{
    // TODO:
    // Move this to right location ?
    public enum ScreenEventTypes
    {
        None,
        Toggle,
        Enable,
        Disable,
        SwitchActiveScreen,
        SwitchActiveSubScreen,
        ReturningScreenToOrigin
    }

    /// <summary>
    /// UI refactor notes: this component basically replaces SwitchScreenState.
    /// </summary>
    public class ScreenEventOnClickComponent : BaseUiObject
    {
        [SerializeField]
        private ScreenEventTypes screenEventToSendOnClick;

        [SerializeField]
        private BaseScreenObject.ScreenType targetScreenType;

        [SerializeField]
        [Header("Optional: Only used for Toggle")]
        private BaseScreenObject.ScreenType senderScreen;

        [SerializeField]
        [Header("Optional: Only required for SwitchActiveSubScreen")]
        private SubScreenState targetSubScreenState;

        private Interactable interactableComponent;

        protected override void OnEnable()
        {
            base.OnEnable();

            interactableComponent = GetComponent<Interactable>();
            interactableComponent.OnClick.AddListener(OnButtonClick);
        }

        protected void OnDisable()
        {
            interactableComponent.OnClick.RemoveListener(OnButtonClick);
        }

        protected override void SubscribeToEventBuses()
        {
            // Not needed
        }

        /// <summary>
        /// For editor tests.
        /// </summary>
        private void OnMouseDown()
        {
            OnButtonClick();
        }

        private void OnButtonClick()
        {
            switch (screenEventToSendOnClick)
            {
                case ScreenEventTypes.None:
                    break;

                case ScreenEventTypes.Toggle:
                    uiEventBus.Publish(new TogglingScreenEvent(targetScreenType, senderScreen));
                    break;

                case ScreenEventTypes.Enable:
                    uiEventBus.Publish(new SettingScreenVisibilityEvent(targetScreenType, true));
                    break;

                case ScreenEventTypes.Disable:
                    uiEventBus.Publish(new SettingScreenVisibilityEvent(targetScreenType, false));
                    break;

                case ScreenEventTypes.SwitchActiveScreen:
                    uiEventBus.Publish(new SwitchingActiveScreenEvent(targetScreenType));
                    break;

                case ScreenEventTypes.SwitchActiveSubScreen:
                    uiEventBus.Publish(new SettingActiveSubScreenEvent(targetScreenType, targetSubScreenState));
                    break;
                
                case ScreenEventTypes.ReturningScreenToOrigin:
                    uiEventBus.Publish(new ReturningScreenToOriginEvent(targetScreenType));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetScreenEventToSendOnClick(ScreenEventTypes newScreenEvent, BaseScreenObject.ScreenType targetScreenType, BaseScreenObject.ScreenType senderScreenType, SubScreenState subScreenState)
        {
            screenEventToSendOnClick = newScreenEvent;
            this.targetScreenType = targetScreenType;
            senderScreen = senderScreenType;
            targetSubScreenState = subScreenState;
        }
    }
}