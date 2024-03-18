using GIGXR.Platform.CommonAssetTypes.CommonComponents;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.UI;
using GIGXR.Utilities;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace GIGXR.Platform.Core.User
{
    /// <summary>
    /// Manages the physical representations of the user in the scene.
    /// </summary>
    public class UserAvatar : BaseUiObject, IPunInstantiateMagicCallback
    {
        [SerializeField, ReadOnly] 
        private string userId;

        public string UserId => userId;

        [SerializeField]
        private TextMeshPro nameTextBox;

        [SerializeField]
        private GameObject labelGameObject;

        [SerializeField] 
        private GameObject headGameObject;

        [SerializeField]
        private Vector3 headOffset;

        public Vector3 HeadOffset => headOffset;
        
        private PhotonView photonView;

        // Dependencies 

        ISessionManager SessionManager { get; set; }

        INetworkManager NetworkManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager, INetworkManager networkManager)
        {
            SessionManager = sessionManager;
            NetworkManager = networkManager;

            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        private void OnDestroy()
        {
            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // Since the RPCs may be buffered, only accept ones from still active players
            if (info.Sender != null)
            {
                photonView = GetComponent<PhotonView>();

                userId = info.Sender.UserId;

                // Position the body under the Calibration root as the local position relative
                // to this root is what all other users will to see
                transform.SetParent(SessionManager.ScenarioManager.AssetManager.CalibrationRootProvider.ContentMarkerRoot);

                // If this is your own avatar head, then it needs to follow your head movements
                if (userId == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString())
                {
                    SetupHeadFollowing();

                    // Don't keep your own label on as it's distracting
                    SetLabelState(false);
                }

                // Add the avatar head to the list of known avatars here when the head is set up and configured for every user
                UserRepresentations.AddAvatarHead(this);

                SetLabelState(UserRepresentations.NameTagsEnabled);
                SetHeadState(UserRepresentations.HeadsEnabled);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetLabelState(bool value)
        {
            if (userId == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString())
            {
                labelGameObject.SetActive(false);
            }
            else
            {
                labelGameObject.SetActive(value);
            }
        }

        public void SetHeadState(bool value)
        {
            if (userId == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString())
            {
                headGameObject.SetActive(false);
            }
            else
            {
                headGameObject.SetActive(value);
            }
        }

        private void SetupHeadFollowing()
        {
            var followHead = gameObject.AddComponent<TransformFollow>();
            followHead.positionOffset = headOffset;
            followHead.Follow(Camera.main.transform);
        }

        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            if (@event.Player.UserId != userId)
            {
                return;
            }

            UserRepresentations.RemoveAvatarHead(this);

            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        protected override void SubscribeToEventBuses()
        {
        }

        #region RPCs

        [PunRPC]
        public void SetupAvatarRPC(string username)
        {
            nameTextBox.text = username;
        }

        #endregion

        #region PublicAPI

        public void SetupAvatar(string username)
        {
            photonView.RPC(nameof(SetupAvatarRPC), RpcTarget.AllBuffered, username);
        }

        #endregion
    }
}