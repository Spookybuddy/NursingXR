namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
    using Photon.Pun;
    using Photon.Realtime;
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// An AssetTypeComponent that allows an asset that ability to restrict what users are able to effect the assets property
    /// over the network. If an asset does not contain this component, the default Authority will be assumed to be HostOnly.
    /// </summary>
    public class
        NetworkAuthorityAssetTypeComponent : BaseAssetTypeComponent<NetworkAuthorityAssetData>,
            IPunOwnershipCallbacks
    {
        // TODO Think about improving Photon references
        private PhotonView photonView;

        private IGigAssetManager AssetManager;

        [InjectDependencies]
        public void Construct(IGigAssetManager assetManager)
        {
            AssetManager = assetManager;
        }

        public void AddAuthority(Guid newUserWithAuthority)
        {
            assetData.authoritySet.runtimeData.Value.Add(newUserWithAuthority.ToString());
        }

        public void RemoveAuthority(Guid stripUserAuthority)
        {
            if (assetData.authoritySet.runtimeData.Value.Contains(stripUserAuthority.ToString()))
            {
                assetData.authoritySet.runtimeData.Value.Remove(stripUserAuthority.ToString());
            }
        }

        public bool HasAuthority(Guid checkUserAuthority)
        {
            switch (assetData.authority.runtimeData.Value)
            {
                case NetworkAuthorityAssetData.Authority.Everyone:
                    return true;
                case NetworkAuthorityAssetData.Authority.HostOnly:
                    return
                        false; // Right now, a host won't generate these requests so it will always be false, but we may want to check against the NetworkManager's host ID
                case NetworkAuthorityAssetData.Authority.AuthoritySet:
                    return assetData.authoritySet.runtimeData.Value.Contains
                        (checkUserAuthority.ToString());
                default:
                    return false;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            PhotonNetwork.AddCallbackTarget(this);

            photonView = GetComponent<PhotonView>();

            var provider = GetComponent<IManipulatorProvider>();

            var objectManipulator = provider?.ObjectManipulator ?? GetComponent<ObjectManipulator>();
            if (objectManipulator != null)
            {
                objectManipulator.OnManipulationStarted.AddListener(RequestOwnershipIfNeeded);
                objectManipulator.OnManipulationEnded.AddListener(ReturnOwnershipIfNeeded);
            }

            var boundsControl = provider?.BoundsControl ?? GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.RotateStarted.AddListener(RequestOwnershipIfNeeded);
                boundsControl.ScaleStarted.AddListener(RequestOwnershipIfNeeded);
                boundsControl.TranslateStarted.AddListener(RequestOwnershipIfNeeded);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            PhotonNetwork.RemoveCallbackTarget(this);

            var provider = GetComponent<IManipulatorProvider>();

            var objectManipulator = provider?.ObjectManipulator ?? GetComponent<ObjectManipulator>();
            if (objectManipulator != null)
            {
                objectManipulator.OnManipulationStarted.RemoveListener(RequestOwnershipIfNeeded);
                objectManipulator.OnManipulationEnded.RemoveListener(ReturnOwnershipIfNeeded);
            }

            var boundsControl = provider?.BoundsControl ?? GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.RotateStarted.RemoveListener(RequestOwnershipIfNeeded);
                boundsControl.ScaleStarted.RemoveListener(RequestOwnershipIfNeeded);
                boundsControl.TranslateStarted.RemoveListener(RequestOwnershipIfNeeded);
            }
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (photonView == null)
            {
                photonView = GetComponent<PhotonView>();
            }

            // Keep the ownership set to Takeover, the host will takeover the assets to start and users will release
            // the objects when they are done, otherwise the host will always take back ownership if they don't have authority
            if(photonView != null)
            {
                photonView.OwnershipTransfer = OwnershipOption.Takeover;
            }
        }
#endif

        private void RequestOwnershipIfNeeded
            (ManipulationEventData _) => RequestOwnershipIfNeeded();

        private void ReturnOwnershipIfNeeded(ManipulationEventData _)
        {
            // Set the position over the network so that when the object is returned to the host, the position value is the current
            // set value and not the lerping position of the host
            if (hasPositionATC)
            {
                attachedInteractable.SetAssetProperty(nameof(PositionAssetData.position), transform.localPosition);
            }

            if (hasRotationATC)
            {
                attachedInteractable.SetAssetProperty(nameof(RotationAssetData.rotation), transform.localRotation);
            }

            if (hasScaleATC)
            {
                attachedInteractable.SetAssetProperty(nameof(ScaleAssetData.scale), transform.localScale);
            }

            ReturnOwnershipIfNeeded();
        }

        private async UniTaskVoid TempDisableTransformUpdates()
        {
            if (hasPositionATC)
            {
                attachedInteractable.CallAssetMethod(nameof(PositionAssetTypeComponent.IgnoreTransformUpdates), new object[] { true });
            }

            // Since we will send a position update at the end of a manipulation, account for both the last position update and that
            // future position update
            await UniTask.Delay(PhotonNetwork.SendRate * 2);

            // If an object is attached, do not set this to true
            if (hasPositionATC)
            {
                attachedInteractable.CallAssetMethod(nameof(PositionAssetTypeComponent.IgnoreTransformUpdates), new object[] { false });
            }
        }

        private void RequestOwnershipIfNeeded()
        {
            // Request ownership so you can manipulate the object
            if (!photonView.IsMine &&
                PhotonNetwork.InRoom)
            {
                photonView.RequestOwnership();
            }
        }

        private void ReturnOwnershipIfNeeded()
        {
            // Return ownership back to the MasterClient when done manipulating the object
            if (photonView.Owner?.UserId != PhotonNetwork.MasterClient?.UserId &&
                PhotonNetwork.InRoom &&
                photonView.IsMine &&
                IsInitialized &&
                // master client might not have authority in AuthoritySet; just let it be passed around when needed
                assetData.authority.runtimeData.Value != NetworkAuthorityAssetData.Authority.AuthoritySet)
            {
                photonView.TransferOwnership(PhotonNetwork.MasterClient);
            }
        }

        [RegisterPropertyChange(nameof(NetworkAuthorityAssetData.authority))]
        private void HandleAuthorityChange(AssetPropertyChangeEventArgs e)
        {
            AuthorityStateUpdated?.Invoke();
        }

        [RegisterPropertyChange(nameof(NetworkAuthorityAssetData.authoritySet))]
        private void HandleAuthoritySetChange(AssetPropertyChangeEventArgs e)
        {
            AuthorityStateUpdated?.Invoke();
        }

        #region BaseAssetTypeComponent Override

        protected override void Setup()
        {

        }

        protected override void Teardown()
        {

        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Network Authority";
            assetData.description.designTimeData.defaultValue
                = "Restricts an asset's property changes to only those who have authority.";

            assetData.authority.designTimeData.defaultValue
                = NetworkAuthorityAssetData.Authority.Everyone;
            assetData.authority.designTimeData.isEditableByAuthor = true;
        }

        #endregion

        #region IPunOwnershipCallbacks

        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            // OnOwnershipRequest gets called on every script that implements it every time a request for ownership transfer of any object occurs
            // So, firstly, only continue if this callback is getting called because *this* object is being transferred
            if (targetView.gameObject != this.gameObject ||
                !targetView.IsMine)
            {
                return;
            }

            if (targetView.Owner != requestingPlayer)
            {
                if (HasAuthority(Guid.Parse(requestingPlayer.UserId)))
                {
                    targetView.TransferOwnership(requestingPlayer);
                }
            }
        }

        public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            // Host related functionality that only they should execute to manage the ownership of this object
            if (AssetManager != null && AssetManager.AssetContext.IsScenarioAuthority)
            {
                // This object has been transfered, but they do not have the authority to do so, take it back
                if ((NetworkAuthorityAssetData.Authority)assetData.authority.GetRuntimePropertyValue() == NetworkAuthorityAssetData.Authority.AuthoritySet &&
                    !HasAuthority(Guid.Parse(photonView.Owner.UserId)))
                {
                    photonView.TransferOwnership(PhotonNetwork.MasterClient);
                }
            }
            
            if (targetView.gameObject != this.gameObject ||
                !targetView.IsMine)
            {
                return;
            }
            else if(targetView.gameObject == this.gameObject && targetView.IsMine)
            {
                // We need the transforms for the local user to be disabled as they will have the latest transform since they moved it,
                // but the host will still be lerping to that position when you return the object
                TempDisableTransformUpdates().Forget();
            }
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            if (targetView.gameObject != this.gameObject ||
                !targetView.IsMine)
            {
                return;
            }

            Debug.LogError($"[NetworkAuthorityAssetTypeComponent] OnOwnershipTransferFailed on {targetView.gameObject} from {senderOfFailedRequest.NickName}");
        }

        #endregion

        #region Public Events

        public UnityEvent AuthorityStateUpdated;

        #endregion
    }
}