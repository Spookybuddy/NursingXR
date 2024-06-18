namespace GIGXR.Platform.Scenarios.GigAssets
{
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Scenarios.EventArgs;
    using Photon.Pun;
    using System;
    using UnityEngine;

    /// <summary>
    /// A local asset type component that provides access to syncing animations across the network.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PhotonAnimatorView))]
    public class AnimatorAssetTypeComponent : LocalAssetTypeComponent, IRuntimeSyncable
    {
        #region EditorVariables

        public PhotonAnimatorView.SynchronizeType defaultSyncronizationType = PhotonAnimatorView.SynchronizeType.Discrete;

        #endregion

        #region PrivateVariables

        private IScenarioManager _scenarioManager;

        private INetworkManager _networkManager;

        private PhotonView _photonView;
        private Animator _animator;
        private PhotonAnimatorView _photonAnimatorView;

        private float previousSpeed;

        #endregion

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManagerReference, INetworkManager networkManager)
        {
            _scenarioManager = scenarioManagerReference;
            _networkManager = networkManager;

            _scenarioManager.ScenarioLoaded += OnScenarioLoaded;
            _scenarioManager.ScenarioPlaying += OnScenarioPlaying;
            // Use ScenarioReset which is called in the same frame as ScenarioStopped but works without PlayEdit Mode
            _scenarioManager.ScenarioReset += OnScenarioReset;
            _scenarioManager.ScenarioPaused += OnScenarioPaused;
        }

        #region UnityMethods

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            _photonAnimatorView = GetComponent<PhotonAnimatorView>();
            _photonView = _photonAnimatorView.photonView;

            for (int n = 0; n < _animator.layerCount; n++)
            {
                _photonAnimatorView.SetLayerSynchronized(n, defaultSyncronizationType);
            }

            foreach(var parameter in _animator.parameters)
            {
                _photonAnimatorView.SetParameterSynchronized(parameter.name, (PhotonAnimatorView.ParameterType)parameter.type, defaultSyncronizationType);
            }

            // Allows us to reset the animations by disabling the component
            _animator.keepAnimatorStateOnDisable = false;
        }

        private void OnDestroy()
        {
            if (_scenarioManager != null)
            {
                _scenarioManager.ScenarioLoaded -= OnScenarioLoaded;
                _scenarioManager.ScenarioPlaying -= OnScenarioPlaying;
                _scenarioManager.ScenarioReset -= OnScenarioReset;
                _scenarioManager.ScenarioPaused -= OnScenarioPaused;
            }
        }

        #endregion

        #region ScenarioEvents

        private void OnScenarioLoaded(object sender, ScenarioLoadedEventArgs e)
        {
            ResetAnimator();
        }

        private void OnScenarioPlaying(object sender, ScenarioPlayingEventArgs e)
        {
            _animator.speed = previousSpeed != 0 ? previousSpeed : 1.0f;

            previousSpeed = 0;
        }

        private void OnScenarioReset(object sender, ScenarioResetEventArgs e)
        {
            ResetAnimator();
        }

        private void OnScenarioPaused(object sender, ScenarioPausedEventArgs e)
        {
            PauseAnimator();
        }

        #endregion

        private void ResetAnimator()
        {
            _animator.enabled = false;

            _animator.Rebind();

            _animator.enabled = true;

            PauseAnimator();
        }

        private void PauseAnimator()
        {
            previousSpeed = _animator.speed;

            _animator.speed = 0;
        }

        public void Sync()
        {
            int[] currentAnimationTimes = new int[_animator.layerCount];
            int[] currentAnimationHashes = new int[_animator.layerCount];
            float[] currentAnimationLengths = new float[_animator.layerCount];

            for(int layer = 0; layer < _animator.layerCount; layer++)
            {
                var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(layer);

                double normalizedAnimationTime;

                // Special case for animations that aren't looping
                if(!currentStateInfo.loop)
                {
                    // This animation isn't looping and since the normalized value is greater than 1, we know it has completed the animation
                    if(currentStateInfo.normalizedTime >= 1.0f)
                    {
                        normalizedAnimationTime = 1.0;
                    }
                    // Otherwise, the animation is currently playing, establish the normalized time
                    else
                    {
                        normalizedAnimationTime = currentStateInfo.normalizedTime - Math.Truncate(currentStateInfo.normalizedTime);
                    }
                }
                else
                {
                    // For some reason, the normalizedTime is a monotonically increasing value that says how many times it has played (i.e. 2.5 means that
                    // it has played the animations 2 and half times), we just need the point in the state, so grab just the decimal place of the number
                    normalizedAnimationTime = currentStateInfo.normalizedTime - Math.Truncate(currentStateInfo.normalizedTime);
                }
                
                // Multiply by 1000 since AnimatorStateInfo.Length is in seconds and we need ms
                currentAnimationTimes[layer] = (int)(1000 * currentStateInfo.length * normalizedAnimationTime);
                currentAnimationHashes[layer] = currentStateInfo.fullPathHash;
                currentAnimationLengths[layer] = currentStateInfo.length;
            }
            
            _photonView.RPC
            (
                nameof(SyncRPC),
                RpcTarget.All,
                currentAnimationTimes,
                currentAnimationHashes,
                currentAnimationLengths
            );
        }

        [PunRPC]
        private void SyncRPC(int[] layerTimes, int[] hashLayerNames, float[] animationLengthsSeconds, PhotonMessageInfo info)
        {
            // Check to see when the message was sent vs when it actually is so the clock is actually in sync with
            // the delay over the network accounted for
            var offset = _networkManager != null ? Math.Abs(_networkManager.ServerTime - info.SentServerTimestamp) : 0;
            for (int n = 0; n < layerTimes.Length; n++)
            {
                // Convert the current place in the animation (in ms) to the lag compensated, normalized position in seconds
                var normalizedLagTime = ((layerTimes[n] + offset) / 1000.0f) / animationLengthsSeconds[n];
                
                _animator.Play(hashLayerNames[n], n, normalizedLagTime);
            }
        }
    }
}