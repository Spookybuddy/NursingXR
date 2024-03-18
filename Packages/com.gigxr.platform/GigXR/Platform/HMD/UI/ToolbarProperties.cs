namespace GIGXR.Platform.HMD.UI
{
    using Platform.AppEvents.Events.Authentication;
    using Platform.AppEvents.Events.Session;
    using GIGXR.Platform.UI;
    using System.Collections.Generic;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using UnityEngine;
    using GIGXR.Platform.Interfaces;
    using System.Collections;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Sessions;
    using System.Linq;
    using GIGXR.Platform.AppEvents.Events;
    using Microsoft.MixedReality.Toolkit.UI;

    /// <summary>
    /// Manages the visual state of the toolbar.
    /// </summary>
    [RequireComponent(typeof(ManipulationWatcher))]
    public class ToolbarProperties : BaseUiObject, IScrollInput
    {
        #region Serialized Fields

        [SerializeField]
        private float distanceInFrontOfUser = 1.0f;

        [SerializeField]
        private int numberOfButtonsToDisplay = 4;

        [SerializeField]
        private int buttonMovementAmount = 1;

        [SerializeField]
        private GameObject displayRoot;

        [SerializeField]
        private GridObjectCollection grid;

        [SerializeField]
        private GameObject scrollLeftButton;

        [SerializeField]
        private GameObject scrollRightButton;

        [SerializeField]
        private GameObject ToolbarButtonPrefab;

        #endregion

        public bool ToolbarState { get { return displayRoot.activeInHierarchy; } }

        private Transform UserCamera
        {
            get
            {
                if (_userCamera == null)
                    _userCamera = Camera.main.transform;

                return _userCamera;
            }
        }

        private Transform _userCamera;

        private ISessionManager SessionManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager)
        {
            SessionManager = sessionManager;
        }

        #region Private Variables

        private int toolbarButtonScrollIndex;

        private Dictionary<BaseScreenObject.ScreenType, ToolbarButton> toolbarButtons = new Dictionary<BaseScreenObject.ScreenType, ToolbarButton>();

        private HashSet<BaseScreenObject.ScreenType> supressedToolbarButtons = new HashSet<BaseScreenObject.ScreenType>();

        #endregion

        #region UnityAPI

        private void OnApplicationQuit()
        {
            uiEventBus.Unsubscribe<SetToolbarStateEvent>(OnSetToolbarStateEvent);
            uiEventBus.Unsubscribe<TryToUndockScreenFromToolbarEvent>(OnTryToUndockScreenFromToolbarEvent);
            uiEventBus.Unsubscribe<AddScreenToToolbarEvent>(OnAddScreenToToolbarEvent);
            uiEventBus.Unsubscribe<RemoveScreenFromToolbarEvent>(OnRemoveScreenFromToolbarEvent);
            uiEventBus.Unsubscribe<ScreenPlacementHintEvent>(OnScreenPlacementHintEvent);
            uiEventBus.Unsubscribe<AddAccessoryElementToolbarEvent>(OnAddAccessoryElementToolbarEvent);
            uiEventBus.Unsubscribe<SetToolbarButtonsStateEvent>(OnSetToolbarButtonsStateEvent);

            EventBus.Unsubscribe<StartLogOutEvent>(DisableToolbar);
            EventBus.Unsubscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
            EventBus.Unsubscribe<LeftSessionEvent>(OnLeftSessionEvent);
            EventBus.Unsubscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
        }

        #endregion

        #region BaseUiObject Implementation

        protected override void SubscribeToEventBuses()
        {
            uiEventBus.Subscribe<SetToolbarStateEvent>(OnSetToolbarStateEvent);
            uiEventBus.Subscribe<TryToUndockScreenFromToolbarEvent>(OnTryToUndockScreenFromToolbarEvent);
            uiEventBus.Subscribe<AddScreenToToolbarEvent>(OnAddScreenToToolbarEvent);
            uiEventBus.Subscribe<RemoveScreenFromToolbarEvent>(OnRemoveScreenFromToolbarEvent);
            uiEventBus.Subscribe<ScreenPlacementHintEvent>(OnScreenPlacementHintEvent);
            uiEventBus.Subscribe<AddAccessoryElementToolbarEvent>(OnAddAccessoryElementToolbarEvent);
            uiEventBus.Subscribe<SetToolbarButtonsStateEvent>(OnSetToolbarButtonsStateEvent);

            EventBus.Subscribe<StartLogOutEvent>(DisableToolbar);
            EventBus.Subscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
            EventBus.Subscribe<LeftSessionEvent>(OnLeftSessionEvent);
            EventBus.Subscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Enables or disables the toolbar.
        /// </summary>
        private void OnSetToolbarStateEvent(SetToolbarStateEvent @event)
        {
            displayRoot.SetActive(@event.ToolbarState);
        }

        private void OnPinButtonClicked(ToolbarButton toolbarButton)
        {
            toolbarButton.SetAccessoryState(false);
        }

        private void OnTryToUndockScreenFromToolbarEvent(TryToUndockScreenFromToolbarEvent e)
        {
            // The toolbar must be active and have the screen button in order to active the return to toolbar button
            // e.g the toolbar button is not active during the first calibration, which should not trigger an undocked event
            if (ToolbarState && toolbarButtons.ContainsKey(e.UndockedScreen))
            {
                toolbarButtons[e.UndockedScreen].SetAccessoryState(true);

                uiEventBus.Publish(new UndockedScreenFromToolbarEvent(e.UndockedScreen));
            }
            // If the toolbar is inactive, we still want the screen to be 'released' from the toolbar since it is still set to follow
            // it, but we do not want to 'pin' button to activate as the toolbar is currently hidden
            else if(!ToolbarState && toolbarButtons.ContainsKey(e.UndockedScreen))
            {
                uiEventBus.Publish(new UndockedScreenFromToolbarEvent(e.UndockedScreen));
            }
        }

        /// <summary>
        /// Adds a new button to the toolbar that is linked to a BaseScreenObject
        /// </summary>
        /// <param name="event">The event that holds the UI data for the button information</param>
        private void OnAddScreenToToolbarEvent(AddScreenToToolbarEvent @event)
        {
            var newToolbarButton = Instantiate(ToolbarButtonPrefab, grid.gameObject.transform);
            newToolbarButton.name = @event.ToolbarButtonInformation.name + " Button";

            var toolbarButton = newToolbarButton.GetComponent<ToolbarButton>();

            toolbarButtons.Add(@event.ScreenType, toolbarButton);

            if (@event.ToolbarButtonInformation.ScreenTypeOverride != BaseScreenObject.ScreenType.None)
                supressedToolbarButtons.Add(@event.ToolbarButtonInformation.ScreenTypeOverride);

            toolbarButton.Setup(this, @event.ScreenType, @event.ToolbarButtonInformation);

            if (toolbarButton.ToolbarButtonInfo.isButtonEnabled)
            {
                void PinButtonClickedAction(ButtonComponent sender)
                {
                    OnPinButtonClicked(toolbarButton);
                }

                // TODO The pinnable screen component should probably handle the actual OnClick method
                // instead of the toolbar
                var pinButton = toolbarButton.AssociatedPinButton.GetComponent<ButtonComponent>();
                pinButton.OnClick += PinButtonClickedAction;
            }
            else
            {
                // Assumes this button will not be active, and will not be activated at runtime.
                toolbarButton.gameObject.SetActive(false);
            }

            UpdateToolbarButtons();
        }

        /// <summary>
        /// Removes the button linked to the given screen type.
        /// </summary>
        /// <param name="event">The event that holds the screen type to remove</param>
        private void OnRemoveScreenFromToolbarEvent(RemoveScreenFromToolbarEvent @event)
        {
            if (toolbarButtons.ContainsKey(@event.ScreenType))
            {
                if (toolbarButtons[@event.ScreenType].ToolbarButtonInfo.ScreenTypeOverride != BaseScreenObject.ScreenType.None)
                {
                    supressedToolbarButtons.Remove(toolbarButtons[@event.ScreenType].ToolbarButtonInfo.ScreenTypeOverride);
                }

                // Since the toolbar creates the button, it must destroy it as well
                Destroy(toolbarButtons[@event.ScreenType]);

                toolbarButtons.Remove(@event.ScreenType);
            }
        }

        private void OnJoinedSessionEvent(BaseSessionStatusChangeEvent e)
        {
            StartCoroutine(UpdateToolbarAfterHostIsKnown());
        }

        private void OnHostTransferCompleteEvent(HostTransferCompleteEvent @event)
        {
            // After a new host has been selected, check to see if the local user is the host, and if so, update their buttons (and screens)
            UpdateToolbarButtons();

            // If the user isn't a host, make sure they do not have any host windows left open
            foreach (var currentToolbarButton in toolbarButtons.ToArray())
            {
                if (currentToolbarButton.Value.ToolbarButtonInfo.isHostOnly && !SessionManager.IsHost)
                {
                    uiEventBus.Publish(new SettingScreenVisibilityEvent(currentToolbarButton.Key, false));
                }
            }
        }

        private void OnScreenPlacementHintEvent(ScreenPlacementHintEvent @event)
        {
            if (@event.IncludeToolbar)
            {
                transform.position = @event.WorldPosition;
                transform.rotation = @event.WorldRotation;
            }
        }

        private void OnAddAccessoryElementToolbarEvent(AddAccessoryElementToolbarEvent @event)
        {
            if(@event.ToolbarScreenTypeButton == BaseScreenObject.ScreenType.None)
            {
                // Special case for none, not used at the moment
            }
            else if(toolbarButtons.ContainsKey(@event.ToolbarScreenTypeButton))
            {
                // Take the accessory element 
                toolbarButtons[@event.ToolbarScreenTypeButton].AddAccessory(@event.AccessoryElement);
            }
        }

        private void OnSetToolbarButtonsStateEvent(SetToolbarButtonsStateEvent @event)
        {
            foreach (var currentToolbarButton in toolbarButtons.ToArray())
            {
                var interactable = currentToolbarButton.Value.gameObject.GetComponent<Interactable>();
                interactable.SetState(InteractableStates.InteractableStateEnum.Disabled, @event.State);
            }
        }

        private IEnumerator UpdateToolbarAfterHostIsKnown()
        {
            while (SessionManager.HostId == System.Guid.Empty)
            {
                yield return null;
            }

            UpdateToolbarButtons();
        }

        private void OnLeftSessionEvent(BaseSessionStatusChangeEvent e)
        {
            // Iterate through all the known buttons, if a session button is open when leaving a session, bring it down
            // because the button will not be available for the user to do so
            foreach (var currentToolbarButton in toolbarButtons.ToArray())
            {
                if (currentToolbarButton.Value.ToolbarButtonInfo.onlyVisibleInsideSession)
                {
                    uiEventBus.Publish(new SettingScreenVisibilityEvent(currentToolbarButton.Key, false));
                }
            }

            UpdateToolbarButtons();
        }

        private void DisableToolbar(StartLogOutEvent e)
        {
            displayRoot.SetActive(false);
        }

        #endregion

        /// <summary>
        /// Positions the toolbar in front on the user, based on the camera position.
        /// 
        /// Called via the Unity Editor.
        /// </summary>
        public void SetInFrontOfUser()
        {
            // Position the toolbar based on the distance set in the Editor
            transform.position = UserCamera.transform.position + (UserCamera.transform.forward * distanceInFrontOfUser);

            // Keep the toolbar level, simply have it point in the same direction as the camera in the y direction
            transform.rotation = Quaternion.Euler(new Vector3(0.0f,
                                                              UserCamera.transform.rotation.eulerAngles.y,
                                                              0.0f));
        }

        public bool IsScreenTypeSupressed(BaseScreenObject.ScreenType screenType)
        {
            return supressedToolbarButtons.Contains(screenType);
        }

        #region IScrollInputImplementation

        public void ScrollUp()
        {
            if ((toolbarButtonScrollIndex + buttonMovementAmount) < toolbarButtons.Count)
            {
                toolbarButtonScrollIndex += buttonMovementAmount;
            }

            UpdateToolbarButtons();
        }

        public void ScrollDown()
        {
            if ((toolbarButtonScrollIndex - buttonMovementAmount) >= 0)
            {
                toolbarButtonScrollIndex -= buttonMovementAmount;
            }

            UpdateToolbarButtons();
        }

        /// <summary>
        /// Iterates through all the buttons and enables the ones that are in the viewing window size as well as
        /// to other properties such as onlyVisibleInsideSession.
        /// This updates the scroll buttons if they are needed or not as well.
        /// </summary>
        private void UpdateToolbarButtons()
        {
            int currentActivatedCount = 0;
            int activatableCount = 0;

            // Iterate over the buttons in the given order so they activate from the start of the list
            var toolbarButtonsInOrder = toolbarButtons.Values.OrderBy(n => n.OrderInToolbar);

            // Activate the buttons that are in the view window, while deactivating the others
            foreach (var currentButton in toolbarButtonsInOrder)
            {
                // Place the toolbar button in the order requested
                currentButton.transform.SetSiblingIndex(currentButton.OrderInToolbar);
                
                if(currentButton.IsActivatable)
                {
                    activatableCount++;
                }

                if (currentButton.TryToActivate(currentActivatedCount, toolbarButtonScrollIndex, numberOfButtonsToDisplay))
                {
                    currentActivatedCount++;
                }
            }

            // If the user has ever scrolled right, then this button needs to be available, otherwise it's at the start of the list and can be disabled
            scrollLeftButton.SetActive(toolbarButtonScrollIndex != 0);

            // The user will need to scroll right if the number of toolbar buttons is greater than the view window and they haven't reached the end of the list
            scrollRightButton.SetActive(grid.transform.childCount > numberOfButtonsToDisplay &&
                                        currentActivatedCount == numberOfButtonsToDisplay &&
                                        activatableCount > numberOfButtonsToDisplay);

            StartCoroutine(UpdateGridCollection());
        }

        /// <summary>
        /// Delays updating the GridObjectCollection until the next frame
        /// </summary>
        private IEnumerator UpdateGridCollection()
        {
            yield return null;

            // The grid needs to be manually updated after adding an object to it
            grid.UpdateCollection();
        }

        #endregion
    }
}