using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.Utilities;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.Networking.EventBus.Events;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.AppEvents.Events;
using GIGXR.Platform.UI;
using GIGXR.Platform.Core.DependencyInjection;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.AppEvents.Events.Session;

namespace GIGXR.Platform.Core.User
{
    public class PingEventArgs : EventArgs
    {
        public int Ping { get; }
        public PingStatus Status { get; }

        public PingEventArgs(int ping, PingStatus pingStatus)
        {
            Ping   = ping;
            Status = pingStatus;
        }
    }

    // TODO remove IPunInstantiateMagicCallback dependency
    public class UserCard : BaseUiObject, IPunInstantiateMagicCallback
    {
        #region Static

        public static int MaxUserCardsToShow { get; private set; }

        private static int firstUserCardIndex;

        private static int endUserCardIndex
        {
            get { return firstUserCardIndex + MaxUserCardsToShow; }
        }

        private static Transform parentTransform;

        private static GridObjectCollection userGridCollection
        {
            get 
            {
                if (parentTransform != null)
                    return parentTransform.GetComponent<GridObjectCollection>();
                else
                    return null;
            }
        }

        public static void SetupAllUserCards(Transform userGrid, int maxUserCards)
        {
            parentTransform    = userGrid;
            MaxUserCardsToShow = maxUserCards;
            OrganizeUserCards();
        }

        private static Dictionary<string, UserCard> userIdToUserCard = new Dictionary<string, UserCard>();
        private static IEnumerable<UserCard> AllUserCards => userIdToUserCard.Values;

        private INetworkManager NetworkManager { get; set; }
        protected ISessionManager SessionManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager, INetworkManager networkManager)
        {
            NetworkManager = networkManager;
            SessionManager = sessionManager;

            NetworkManager.Subscribe<PingValueUpdatedNetworkEvent>(SetPingStatus);
            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
        }

        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            if(@event.Player.UserId == userId.ToString())
            {
                if(photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }

                OrganizeUserCards();
            }
        }

        protected void OnHostTransferCompleteEvent(HostTransferCompleteEvent @event)
        {
            var previousHostUserCard = FindUserCard(@event.PreviousHostId.ToString());

            // A new host has been set, remove the host label on the previous host
            if (previousHostUserCard != null)
            {
                previousHostUserCard.UpdateUserHostStatus(false);
            }

            var localUserCard = FindUserCard(SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString());

            // Set your own card
            if (localUserCard != null)
            {
                localUserCard.UpdateUserHostStatus(SessionManager.IsHost);
            }

            var newHostUserCard = FindUserCard(@event.NewHostId.ToString());

            // Set the new host card
            if (SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId != @event.NewHostId &&
                newHostUserCard != null)
            {
                newHostUserCard.UpdateUserHostStatus(true);
            }
        }

        private static UserCard FindUserCard(string userId)
        {
            if (userIdToUserCard.TryGetValue(userId, out UserCard userCard))
            {
                return userCard;
            }
            return null;
        }

        public static void ScrollDown(int scrollAmount, int maxCount)
        {
            int prevIndex = firstUserCardIndex;
            firstUserCardIndex += scrollAmount;

            if (firstUserCardIndex > maxCount)
                firstUserCardIndex = prevIndex;

            OrganizeUserCards();
        }

        public static void ScrollUp(int scrollAmount)
        {
            firstUserCardIndex -= scrollAmount;

            if (firstUserCardIndex < 0)
                firstUserCardIndex = 0;

            OrganizeUserCards();
        }

        /// <summary>
        /// Generates a user card for the input user details.
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="userId"></param>
        /// <param name="colocated"></param>
        public static UserCard GenerateUserCard(string nickName, string userId)
        {
            // TODO Move PhotonReference outside of UserCard
            var userCardGO = PhotonNetwork.Instantiate
            (
                "UserCard",
                Vector3.zero,
                Quaternion.identity
            );

            var currentPhotonView = userCardGO.GetPhotonView();

#if UNITY_IOS || UNITY_ANDROID
            currentPhotonView?.RPC
            (
                nameof(SetDeviceStatus),
                RpcTarget.AllBuffered,
                "Mobile"
            );
#else
            currentPhotonView?.RPC
            (
                nameof(SetDeviceStatus),
                RpcTarget.AllBuffered,
                "Headset"
            );
#endif

            userCardGO.name = nickName;

            return userCardGO.GetComponent<UserCard>();
        }

        public async static void OrganizeUserCards()
        {
            foreach (UserCard currentUserCard in AllUserCards)
            {
                currentUserCard.transform.SetParent(parentTransform);
                currentUserCard.transform.localRotation = Quaternion.identity;

                bool active = currentUserCard.transform.GetSiblingIndex() >= firstUserCardIndex &&
                              currentUserCard.transform.GetSiblingIndex() < endUserCardIndex;

                currentUserCard.gameObject.SetActive(active);
            }

            await UniTask.Yield();

            userGridCollection?.UpdateCollection();
        }

        // TODO Get this Photon reference out of here
        public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // Since the RPCs may be buffered, only accept ones from still active players
            if(info.Sender != null)
            {
                bool isSessionHost = SessionManager.HostId == Guid.Parse(info.Sender.UserId);

                Configure
                (
                    info.Sender.NickName,
                    info.Sender.UserId,
                    isSessionHost
                );

                OrganizeUserCards();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        // --- Events:

        public event EventHandler<PingEventArgs> PingUpdatedViaRPC;

        // --- Serialized Variables:

        [SerializeField]
        private TextMeshProUGUI usernameDisplay;

        [SerializeField]
        private GameObject hostLabel;

        [SerializeField]
        protected ButtonComponent showUserProfileButton;

        [Header("Icon Assets")]
        [SerializeField]
        private LabeledIconCollectionScriptableObject avatarIconObjects;

        [Header("Icon MonoBehaviors")]
        // Network icon is not going to change sprites, so no need for a LabeledIconScriptableObject
        [SerializeField]
        private LabeledIcon networkIconObject;

        [SerializeField]
        private LabeledIcon avatarIcon;

        // --- Private Variables:

        [SerializeField]
        protected string userId;

        // TODO Abstract this dependency away from Photon
        private PhotonView photonView;

        private int lastPingValue;

        private string _deviceString;

        public string DeviceString { get { return _deviceString; } }

        public bool IsUserUsingMobile { get { return DeviceString == "Mobile"; } }

        // --- Public Properties:

        public Guid UserGUID 
        { 
            get 
            { 
                if(string.IsNullOrEmpty(userId))
                    return Guid.Empty;
                else
                    return Guid.Parse(userId); 
            } 
        }

        public int LastPing { get { return lastPingValue; } }
        public Color NetworkIconColor { get { return networkIconObject.IconImage.color; } }

        // --- Unity Methods:

        private void Start()
        {
            if (showUserProfileButton != null)
            {
                showUserProfileButton.OnClick += ShowUserProfileButtonClicked;
            }
        }

        protected virtual void OnDestroy()
        {
            userIdToUserCard.Remove(userId);

            OrganizeUserCards();

            if (showUserProfileButton != null)
            {
                showUserProfileButton.OnClick -= ShowUserProfileButtonClicked;
            }

            EventBus.Unsubscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);

            NetworkManager.Unsubscribe<PingValueUpdatedNetworkEvent>(SetPingStatus);
            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        // --- Public Methods:

        /// <summary>
        /// Configures the card with the user's name and their location status.
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="colocated"></param>
        public void Configure
        (
            string nickname,
            string id,
            bool isSessionHost = false
        )
        {
            // paranoic: ensure configure is not called 
            if (!string.IsNullOrEmpty(userId))
            {
                if (userIdToUserCard.Remove(userId))
                {
                    Debug.LogWarning($"[UserCard] Duplicate configuration for card with id {userId} and new configuration id {id}. Configuration skipped.");
                    return;
                }
            }

            // paranoic, ensure ids are not reused
            if (userIdToUserCard.ContainsKey(id))
            {
                Debug.LogWarning($"[UserCard] Attempted to configure UserCard with duplicate id {id}. Configuration skipped.");
                return;
            }

            name   = nickname;
            userId = id;

            // add this card to the static lookup
            userIdToUserCard.Add(id, this);

            transform.SetParent(parentTransform);
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition    = Vector3.zero;

            // set the username display and whether the user is collocated or not.
            usernameDisplay.SetText(nickname);

            hostLabel.SetActive(isSessionHost);

            photonView = GetComponent<PhotonView>();
        }

        protected virtual void ShowUserProfileButtonClicked(ButtonComponent sender)
        {
            var userGuid = sender.GetComponentInParent<UserCard>().UserGUID;

            EventBus.Publish(new OpenUserProfileEvent(userGuid, this));
        }

        protected virtual void UpdateUserHostStatus(bool isHost)
        {
            hostLabel.SetActive(isHost);
        }

        [PunRPC]
        public void SetDeviceStatus(string device)
        {
            _deviceString = device;

            avatarIcon.Configure(avatarIconObjects.GetLabelIcon(device));
        }

        public void SetPingStatus(PingValueUpdatedNetworkEvent @event)
        {
            // Every UserCard is listening for this event, but it should only be applied to your own card
            if(UserGUID == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId)
            {
                photonView.RPC
                (
                    nameof(UpdatePing),
                    RpcTarget.All,
                    @event.PingValue,
                    @event.PingStatus
                );
            }
        }

        // TODO These should be abstract from Photon in some way

        #region PhotonRPCMethods

        [PunRPC]
        private void UpdatePing(int pingValue, PingStatus pingStatus)
        {
            lastPingValue = pingValue;

            if (pingStatus == PingStatus.Severe)
            {
                // Set the network icon to red to indicate to all users that your network is severe
                networkIconObject.SetIconColor(Color.red);
            }
            else
            {
                networkIconObject.SetIconColor(Color.white);
            }

            PingUpdatedViaRPC?.Invoke(this, new PingEventArgs(pingValue, pingStatus));
        }

        #endregion
    }
}