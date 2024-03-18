namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.Sessions;
    using GIGXR.Platform.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Helps with the creation of buttons for the toolbar at runtime and hooks up the needed MonoBehaviors on the GameObject.
    /// </summary>
    [RequireComponent(typeof(ScreenEventOnClickComponent))]
    [RequireComponent(typeof(ButtonComponent))]
    public class ToolbarButton : BaseUiObject
    {
        #region EditorVariables

        [SerializeField]
        private GridObjectCollection accessoryButtonHolder;

        [SerializeField]
        private GameObject associatedPinButton;

        [SerializeField]
        private TMPro.TextMeshProUGUI buttonText;

        [SerializeField]
        private Image buttonImage;

        #endregion

        private int orderInList;

        public int OrderInToolbar { get { return orderInList; } }

        private ToolbarProperties toolbar;

        private ScreenEventOnClickComponent screenEventClickComponent;

        private ButtonComponent buttonComponent;

        public BaseScreenObject.ScreenType ConnectedScreenType { get; private set; }

        public GameObject AssociatedPinButton { get { return associatedPinButton; } }

        public ToolbarButtonScriptableObject ToolbarButtonInfo { get; private set; }

        public bool IsActivatable
        {
            get
            {
                return (ToolbarButtonInfo.onlyVisibleInsideSession && 
                        SessionManager?.ActiveSession != null) ||
                       !ToolbarButtonInfo.onlyVisibleInsideSession;
            }
        }

        private ISessionManager SessionManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager)
        {
            SessionManager = sessionManager;
        }

        private void OnDestroy()
        {
            uiEventBus.Unsubscribe<SetAccessoryElementStateToolbarEvent>(OnSetAccessoryElementStateToolbarEvent);
        }

        #region PublicAPI

        /// <summary>
        /// Connects the UI and MRTK related classes with the data that was set in the Editor.
        /// </summary>
        /// <param name="screenType">The ScreenType of the connected screen.</param>
        /// <param name="buttonInformation">UI data in a ScriptableObject</param>
        public void Setup(ToolbarProperties toolbar, BaseScreenObject.ScreenType screenType, ToolbarButtonScriptableObject buttonInformation)
        {
            this.toolbar = toolbar;

            screenEventClickComponent = GetComponent<ScreenEventOnClickComponent>();
            buttonComponent = GetComponent<ButtonComponent>();

            ConnectedScreenType = screenType;
            ToolbarButtonInfo = buttonInformation;

            if(!string.IsNullOrEmpty(buttonInformation.speechCommand))
            {
                buttonComponent.SetSpeechKeyword(buttonInformation.speechCommand);
            }

            screenEventClickComponent.SetScreenEventToSendOnClick(ScreenEventTypes.Toggle, 
                                                                  screenType, 
                                                                  screenType, 
                                                                  SubScreenState.None);

            var pinnableButtonScreenEvent = associatedPinButton.GetComponent<ScreenEventOnClickComponent>();
            pinnableButtonScreenEvent.SetScreenEventToSendOnClick(ScreenEventTypes.ReturningScreenToOrigin, 
                                          screenType, 
                                          BaseScreenObject.ScreenType.None, 
                                          SubScreenState.None);

            buttonText.text = buttonInformation.iconInformation.iconName;

            buttonImage.sprite = buttonInformation.iconInformation.iconSprite;

            if(buttonInformation.iconInformation.iconScale != Vector3.zero)
            {
                buttonImage.transform.localScale = buttonInformation.iconInformation.iconScale;
            }

            orderInList = buttonInformation.orderInToolbar;

            TryToConstruct();
        }

        /// <summary>
        /// Attempts to enable the button if the button is in view
        /// </summary>
        /// <param name="enabledCount">How many buttons have already been enabled</param>
        /// <param name="toolbarButtonScrollIndex">The current scroll index in the list of toolbar buttons</param>
        /// <param name="maxButtons">The total number of visible buttons</param>
        /// <returns></returns>
        public bool TryToActivate(int enabledCount, int toolbarButtonScrollIndex, int maxButtons)
        {
            // This is a session button and you are in a session OR you are not a session button and can be displayed anytime
            bool validButton = IsActivatable;

            // The button in the order has to be greater than the index
            validButton &= transform.GetSiblingIndex() >= toolbarButtonScrollIndex;

            // The button is for the host and you are the host, or the button is not just for the host
            if (SessionManager?.ActiveSession != null)
            {
                validButton &= ((ToolbarButtonInfo.isHostOnly && SessionManager.IsHost) ||
                           !ToolbarButtonInfo.isHostOnly);
            }

            validButton &= !toolbar.IsScreenTypeSupressed(ConnectedScreenType);

            // Then use the place in index to determine if the object should be active
            gameObject.SetActive(enabledCount < maxButtons &&
                                 validButton);

            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Adds an accessory (a UI button underneath this button) to
        /// the toolbar button. (e.g. the pin button)
        /// </summary>
        /// <param name="accessoryElement"></param>
        public void AddAccessory(GameObject accessoryElement)
        {
            accessoryElement.transform.SetParent(accessoryButtonHolder.transform, false);

            accessoryButtonHolder.UpdateCollection();
        }

        public void SetAccessoryState(bool state)
        {
            foreach(var obj in accessoryButtonHolder.NodeListReadOnly)
            {
                if(obj.Transform != null)
                {
                    obj.Transform.gameObject.SetActive(state);
                }
            }
        }

        #endregion

        private void OnSetAccessoryElementStateToolbarEvent(SetAccessoryElementStateToolbarEvent @event)
        {
            SetAccessoryState(@event.AccessoryState);
        }


        protected override void SubscribeToEventBuses()
        {
            uiEventBus.Subscribe<SetAccessoryElementStateToolbarEvent>(OnSetAccessoryElementStateToolbarEvent);
        }
    }
}