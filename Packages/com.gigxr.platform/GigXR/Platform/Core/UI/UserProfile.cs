namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.GMS.Models.Accounts.Responses;
    using GIGXR.Platform.AppEvents.Events;
    using GIGXR.Platform.AppEvents.Events.Session;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.Networking;
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Tables;
    using System.Threading;
    using GIGXR.Platform.Networking.EventBus.Events.InRoom;
    using GIGXR.Platform.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Core.User;

    public class UserProfile : BaseUiObject
    {
        // --- Serialized Variables:

        [SerializeField]
        private LocalizedStringTable userProfileStringTableAsset;

        [Header("Text Field MonoBehaviors")]

        [SerializeField]
        private TextMeshProUGUI nameTextField;

        [SerializeField]
        private TextMeshProUGUI userRoleTextField;

        [SerializeField]
        protected GameObject hostPanel;

        [SerializeField]
        protected GameObject makeHostButtonGameObject;

        [SerializeField]
        private GameObject reclaimPanel;

        [Header("Icon MonoBehaviors")]

        [SerializeField]
        private LabeledIcon deviceIcon;

        [SerializeField]
        private LabeledIcon pingIcon;

        [SerializeField]
        private TextMeshProUGUI pingValueTextField;

        [Header("Icon Assets")]

        [SerializeField]
        private LabeledIconCollectionScriptableObject deviceIconObjects;

        // The Ping Icon is not expect to change, so it doesn't need the collection
        [SerializeField]
        private LabeledIconScriptableObject pingIconObject;

        // --- Private Variables:

        private INetworkManager NetworkManager { get; set; }

        private StringTable userProfileStringTable;

        protected CancellationTokenSource kickUserPromptSource;

        protected UIPlacementData sessionPromptData;

        protected UserCard attachedUserCard;
        private Guid currentUserId;

        // --- Public Properties:

        public Guid CurrentUserId { get { return currentUserId; } }

        // --- Unity Methods:

        private void Awake()
        {
            userProfileStringTable = userProfileStringTableAsset.GetTable();

            // The Ping icon is not going to change based on the user, so set it now
            pingIcon.Configure(pingIconObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            sessionPromptData = new UIPlacementData()
            {
                HostTransform = transform.parent
            };
        }

        protected virtual void OnDestroy()
        {
            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        // --- Public Methods:

        [InjectDependencies]
        public void Construct(INetworkManager networkManager)
        {
            NetworkManager = networkManager;

            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        protected override void SubscribeToEventBuses()
        {
            // Not needed at the moment
        }

        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            if(kickUserPromptSource != null)
            {
                kickUserPromptSource.Cancel();
                kickUserPromptSource.Dispose();
                kickUserPromptSource = null;
            }
        }

        /// <summary>
        /// Called via the UnityEditor in the UserProfile's Prefab on the Reclaim View's Reclaim Host button.
        /// </summary>
        public void PromptReclaimHost()
        {
            EventBus.Publish(new PromptReclaimHostEvent());
        }

        /// <summary>
        /// Called via the UnityEditor in the UserProfile's Prefab on the Host Panels's Make Host button.
        /// </summary>
        public void PromptMakeUserHost()
        {
            EventBus.Publish(new StartHostRequestEvent(CurrentUserId, nameTextField.text));
        }

        /// <summary>
        /// Called via the UnityEditor in the UserProfile's Prefab on the Host Panels's Remove User button.
        /// </summary>
        /// TODO Logic for removing a user should be abstracted out to SessionManager while prompts handled via SessionScreen
        public void PromptRemoveUser()
        {
            // The host has made a prompt to kick a user, since they are trying to kick someone else (or maybe the same person)
            // simply bring down the last kick user prompt
            if(kickUserPromptSource != null)
            {
                kickUserPromptSource.Cancel();
            }

            kickUserPromptSource = new CancellationTokenSource();

            var yesNoButtonList = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = userProfileStringTable.GetEntry("yesText").GetLocalizedString(),
                        onPressAction = () =>
                        {
                            if (EventBus != null)
                            {
                                kickUserPromptSource.Dispose();
                                kickUserPromptSource = null;

                                EventBus.Publish(new KickUserEvent(currentUserId));
                            }
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = userProfileStringTable.GetEntry("noText").GetLocalizedString(),
                        onPressAction = () =>
                        {
                            kickUserPromptSource.Dispose();
                            kickUserPromptSource = null;
                        }
                    }
                };

            EventBus.Publish
            (
                new ShowCancellablePromptEvent
                (
                    kickUserPromptSource.Token,
                    "",
                    userProfileStringTable.GetEntry("confirmationText").GetLocalizedString(nameTextField.text),
                    yesNoButtonList,
                    sessionPromptData
                )
            );
        }

        /// <summary>
        /// Called via the UnityEditor in the UserProfile's Prefab on the Close button GameObject.
        /// </summary>
        public virtual void CloseUserProfile()
        {
            if (attachedUserCard != null)
            {
                attachedUserCard.PingUpdatedViaRPC -= AttachedUserCard_PingUpdate;
            }

            // Reset all values so they cannot show up again
            attachedUserCard = null;
            currentUserId = Guid.Empty;
            SetUserNameText("");
            SetUserRoleText("");
            UpdatePingText(0);
            deviceIcon.Configure(null);

            ShowHostControls(false);
            makeHostButtonGameObject.SetActive(true);

            // Hide the gameObject so that it doesn't have to be instantiated every time it's needed
            gameObject.SetActive(false);
        }

        public void ShowUserProfile()
        {
            gameObject.SetActive(true);
        }

        public virtual void ShowHostControls(bool panelValue)
        {
            hostPanel.SetActive(panelValue);

            if (attachedUserCard != null && panelValue)
            {
                // If a user profile is pointing to a mobile user, do not show the make host button as they are not allowed to be hosts
                makeHostButtonGameObject.SetActive(!attachedUserCard.IsUserUsingMobile);
            }
        }

        public void ShowReclaimHost(bool panelValue)
        {
            reclaimPanel.SetActive(panelValue);
        }

        public void SetUserDetails(UserCard userCard, AccountDetailedView userAccount)
        {
            // If a card is not closed, this event won't unregister so check here
            if (attachedUserCard != null)
            {
                pingIcon.SetIconColor(Color.white);

                attachedUserCard.PingUpdatedViaRPC -= AttachedUserCard_PingUpdate;
            }

            attachedUserCard = userCard;
            currentUserId = userAccount.AccountId;

            // Initial values
            SetUserNameText($"{userAccount.FirstName} {userAccount.LastName}");
            SetUserRoleText(userAccount.AccountRole.ToString());
            UpdatePingText(attachedUserCard.LastPing);
            pingIcon.SetIconColor(attachedUserCard.NetworkIconColor);

            // The ping will update at it's own frequency, so just listen for the event provided by the UserCard
            attachedUserCard.PingUpdatedViaRPC += AttachedUserCard_PingUpdate;
        }

        private void AttachedUserCard_PingUpdate
        (
            object sender,
            PingEventArgs e
        )
        {
            UpdatePingText(e.Ping);

            if (e.Status == PingStatus.Severe)
            {
                pingIcon.SetIconColor(Color.red);
            }
            else
            {
                pingIcon.SetIconColor(Color.white);
            }
        }

        public void UpdatePingText
        (
            int newPingValue
        )
        {
            pingValueTextField.text = $"{newPingValue} ms";
        }

        public void SetUserNameText
        (
            string userName
        )
        {
            nameTextField.text = userName;
        }

        public void SetUserRoleText
        (
            string role
        )
        {
            userRoleTextField.text = role;
        }

        public void SetDeviceIcon
        (
            string device
        )
        {
            deviceIcon.Configure(deviceIconObjects.GetLabelIcon(device));
        }
    }
}