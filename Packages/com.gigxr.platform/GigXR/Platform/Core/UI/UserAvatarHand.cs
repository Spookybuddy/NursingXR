using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.UI;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Core.User
{
    /// <summary>
    /// Manages the physical representations of the user's hand.
    /// </summary>
    public class UserAvatarHand : BaseUiObject, IPunInstantiateMagicCallback, IMixedRealitySourceStateHandler, IMixedRealityHandJointHandler
    {
        public string UserID => userId;

        [SerializeField, GIGXR.Utilities.ReadOnly]
        private string userId;

        [SerializeField]
        private GameObject handGameObject;

        [SerializeField]
        private Material nudeHandMaterial;

        [SerializeField]
        private Material glovesMaterial;

        [SerializeField]
        private SkinnedMeshRenderer handRenderer;

        private PhotonView photonView;

        private Handedness handedness;

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

            if (photonView.IsMine)
            {
                CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
                CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHandJointHandler>(this);
            }
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

                if (photonView.IsMine)
                {
                    CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
                    CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
                }

                UserRepresentations.AddAvatarHand(this);

                SetHandState(UserRepresentations.HandsEnabled);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            if (@event.Player.UserId == userId)
            {
                UserRepresentations.RemoveAvatarHand(this);

                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        protected override void SubscribeToEventBuses()
        {

        }

        public void SetHandState(bool isActive)
        {
            // If this is your own local hands, do not touch them. Does not mess with the logic above if they are enabled/disabled.
            if (userId != SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString())
            {
                handGameObject.SetActive(isActive);
            }
        }

        public void SetLocalHandState(bool isActive)
        {
            if (userId == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString())
            {
                handGameObject.SetActive(isActive);
            }
        }

        #region RPCs

        [PunRPC]
        public void SetupAvatarHandRPC(byte handedness)
        {
            this.handedness = (Handedness)handedness;
        }

        [PunRPC]
        public void SetAvatarHandRendererRPC(bool isActive)
        {
            if (userId != SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString())
            {
                // Only enable the hands if Hands are enabled locally and this hand itself
                // is activated
                SetHandState(UserRepresentations.HandsEnabled && isActive);
            }
            else
            {
                SetLocalHandState(UserRepresentations.LocalHandsEnabled && isActive);
            }
        }

        [PunRPC]
        public void SetupGlovesRPC(bool visibility, string receivedId)
        {
            if (receivedId == userId)
            {
                if (visibility)
                    EnableGlovesLocally();
                else
                    DisableGlovesLocally();
            }
        }

        #endregion

        #region PublicAPI

        public void SetupAvatarHand(Handedness handedness)
        {
            photonView.RPC
            (
                nameof(SetupAvatarHandRPC),
                RpcTarget.AllBuffered,
                (byte)handedness
            );
        }

        public void ChangeVisibilityOfGlovesNetworked(bool visibility)
        {
            photonView.RPC(
                nameof(SetupGlovesRPC),
                RpcTarget.All,
                visibility,
                userId
            );
        }

        public void EnableGlovesLocally()
        {
            handRenderer.material = glovesMaterial;
        }

        public void DisableGlovesLocally()
        {
            handRenderer.material = nudeHandMaterial;
        }


        public string GetGlovesId()
        {
            return userId;
        }

        #endregion

        #region IMixedRealitySourceStateHandler

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            var hand = eventData.Controller as IMixedRealityHand;

            // Only react to articulated hand input sources
            if (hand != null)
            {
                if (handedness == hand.ControllerHandedness)
                {
                    photonView.RPC
                    (
                        nameof(SetAvatarHandRendererRPC),
                        RpcTarget.All,
                        true
                    );
                }
            }
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            var hand = eventData.Controller as IMixedRealityHand;

            // Only react to articulated hand input sources
            if (hand != null)
            {
                if (handedness == hand.ControllerHandedness)
                {
                    photonView.RPC
                    (
                        nameof(SetAvatarHandRendererRPC),
                        RpcTarget.All,
                        false
                    );
                }
            }
        }

        #endregion

        #region IMixedRealityHandJointHandler 

        // The local user's position will be updated, the PhotonTransformComponent will network it
        public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
        {
            if (handedness == eventData.Handedness)
            {
                MixedRealityPose wristPose;

                if (eventData.InputData.TryGetValue(TrackedHandJoint.Wrist, out wristPose))
                {
                    transform.position = wristPose.Position;
                    transform.rotation = wristPose.Rotation;
                }
            }
        }

        #endregion

    }
}