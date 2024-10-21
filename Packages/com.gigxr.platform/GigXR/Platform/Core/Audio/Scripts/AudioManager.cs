using Cysharp.Threading.Tasks;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
using Logger = GIGXR.Platform.Utilities.Logger;
using GIGXR.Platform.Scenarios.Data;

namespace GIGXR.Platform.Core.Audio
{
    /// <summary>
    /// Async task used for destroying the audio after it finished playing
    /// </summary>
    public class BackgroundWaitAndDestroyHandler : BaseBackgroundHandler
    {
        public AudioSource audioSource;

        protected override async UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            if (audioSource != null)
            {
                float waitTimeForClip = audioSource.clip.length * 1 / audioSource.pitch;
                if (waitTimeForClip < 0)
                    waitTimeForClip = 0f;

                await UniTask.Delay(TimeSpan.FromSeconds(waitTimeForClip), cancellationToken: cancellationToken);

                Object.Destroy(audioSource.gameObject);
            }
            else
            {
                await UniTask.Yield();
            }
        }
    }

    /// <summary>
    /// Audio Manager class that can be injected and called to instantiate any audio in the scene
    /// </summary>
    public class AudioManager : IAudioManager
    {
        // Constants
        private const float VolumeResetDelay = 2;

        //References

        private AudioSource lastAudioCreated;

        private IScenarioManager scenarioManager;

        private AppEventBus appEventBus;

        private AudioMixer masterMixer;

        private Dictionary<string, AudioSource> loopingAudiosDictionary;

        private Queue<AudioSource> audioSourcePool; // queue of available audio sources

        private HashSet<AudioSource> audioPoolMembers = new HashSet<AudioSource>(); // set for membership checking, to determin if an audio source is from the pool or passed into public API from elsewhere.

        private GameObject audioPoolGameObject;

        public AudioDatabase AudioDatabase { get; }

        private BackgroundWaitAndDestroyHandler waitAndDestroyHandler;

        private GigAssetManager assetManager;

        private System.Random random = new System.Random();

        // Omitting master volume as it shouldn't be altered
        public enum MixerParameters
        {
            None = 0,
            PitchHeart,
            VolumeHeart,
            PitchLungs,
            VolumeLungs,
            VolumePatientBreathing,
            HighpassPatientBreathing,
            PitchPatientBreathing,
            VolumePatientSpeech,
            HighpassPatientSpeech,
            VolumeStethoscope
        }

        //Default Constructor - called by the dependency injection manager
        public AudioManager(IScenarioManager scenarioManagerRef, INetworkManager networkManagerRef, AppEventBus appEventBusRef)
        {
            this.scenarioManager = scenarioManagerRef;
            this.appEventBus = appEventBusRef;

            this.scenarioManager.ScenarioLoaded += CreateAudioPool;
            this.scenarioManager.ScenarioLoaded += InitializeAudioDatabase;
            this.scenarioManager.ScenarioPlaying += RaiseMasterMixerVolume;
            this.scenarioManager.ScenarioPaused += MuteMasterMixerVolumePaused;
            this.scenarioManager.ScenarioStopped += MuteMasterMixerVolumeStopped;
            this.scenarioManager.ScenarioUnloaded += DestroyAudioPool;

            this.appEventBus.Subscribe<LeftSessionEvent>(OnLeftSessionDestroyAudios);

            AudioDatabase = Resources.Load<AudioDatabase>("AudioDatabase");
            masterMixer = Resources.Load<AudioMixer>("Mixers/MasterAudioMixer");

            loopingAudiosDictionary = new Dictionary<string, AudioSource>();
        }

        private void InitializeAudioDatabase(object sender, ScenarioLoadedEventArgs e)
        {
            var scenario = (Scenario)scenarioManager.LastSavedScenario;

            Logger.Debug
            (
                $"Trying to initialize new scenario with new scenario name {scenario.scenarioName}",
                "AudioManager"
            );

            AudioDatabase.InitializeAudioDatabase(scenario.scenarioName);
        }

        /// <summary>
        /// Creates an expandable audio pool to use with the AudioManager
        /// </summary>
        private void CreateAudioPool()
        {
            //Starts the pool
            audioSourcePool = new Queue<AudioSource>();
            audioPoolMembers.Clear();

            //Empty game object to hold all the audios from the pool
            audioPoolGameObject = new GameObject { name = "AudioPool" };

            //Creates the first 10 audios inside the pool

            for (int i = 0; i < 10; i++)
            {
                GameObject audioGameObject = new GameObject { name = "AudioSource (" + (i + 1) + "/10)" };
                audioGameObject.transform.SetParent(audioPoolGameObject.transform);
                AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();

                //Add to the pool (queue)
                audioSourcePool.Enqueue(audioSource);
                audioPoolMembers.Add(audioSource);
            }
        }

        private void DestroyAudioPool()
        {
            if (audioPoolGameObject == null)
            {
                return;
            }

            Object.Destroy(audioPoolGameObject);

            audioPoolGameObject = null;
            audioSourcePool.Clear();
            audioPoolMembers.Clear();
        }

        private AudioSource GetAvailableAudioFromThePool()
        {
            bool availableSpot = false;
            AudioSource currentAudioFromThePool = null;
            int poolSize = audioSourcePool.Count;
            int currentPoolCount = 0;

            while (!availableSpot)
            {
                currentAudioFromThePool = audioSourcePool.Dequeue();
                if (!currentAudioFromThePool.isPlaying)
                {
                    availableSpot = true;
                }
                else
                {
                    audioSourcePool.Enqueue(currentAudioFromThePool);
                    currentPoolCount++;

                    if (currentPoolCount < poolSize)
                    {
                        return IncreasePoolSizeWithNewAudioSource(currentAudioFromThePool.transform.parent);
                    }
                }
            }

            return currentAudioFromThePool;
        }

        private AudioSource IncreasePoolSizeWithNewAudioSource(Transform poolParent)
        {
            GameObject audioGameObject = new GameObject { name = "New AudioSource" };
            audioGameObject.transform.SetParent(poolParent);
            AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
            audioPoolMembers.Add(audioSource);

            return audioSource;
        }

        /// <summary>
        /// Methods used for controlling the master mixer volume
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MuteMasterMixerVolumeStopped(object sender, ScenarioStoppedEventArgs e)
        {
            masterMixer.SetFloat("Volume", -80f);
        }

        private void MuteMasterMixerVolumePaused(object sender, ScenarioPausedEventArgs e)
        {
            masterMixer.SetFloat("Volume", -80f);
        }

        private async void RaiseMasterMixerVolume(object sender, ScenarioPlayingEventArgs e)
        {
            masterMixer.SetFloat("Volume", 0f);
            await ResetMixerParameters();
        }

        private void CreateAudioPool(object sender, ScenarioLoadedEventArgs e)
        {
            CreateAudioPool();
        }

        private void DestroyAudioPool(object sender, ScenarioUnloadedEventArgs e)
        {
            StopAllSounds();
            DestroyAudioPool();
        }

        private void OnLeftSessionDestroyAudios(LeftSessionEvent obj)
        {
            StopAllSounds();
            DestroyAudioPool();
        }

        private string GetParameterName(MixerParameters parameter)
        {
            switch (parameter)
            {
                case MixerParameters.PitchHeart:
                    return "PitchHeart";
                case MixerParameters.VolumeHeart:
                    return "VolumeHeart";
                case MixerParameters.PitchLungs:
                    return "PitchLungs";
                case MixerParameters.VolumeLungs:
                    return "VolumeLungs";
                case MixerParameters.HighpassPatientBreathing:
                    return "PatientBreathHighpass";
                case MixerParameters.VolumePatientBreathing:
                    return "PatientBreathVolume";
                case MixerParameters.PitchPatientBreathing:
                    return "PitchPatientBreathing";
                case MixerParameters.VolumePatientSpeech:
                    return "PatientSpeechVolume";
                case MixerParameters.HighpassPatientSpeech:
                    return "PatientSpeechHighpass";
                case MixerParameters.VolumeStethoscope:
                    return "StethoscopeVolume";
                case MixerParameters.None:
                default:
                    return "";
            }
        }

        private async UniTask ResetMixerParameters()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(VolumeResetDelay), ignoreTimeScale: true);
            Logger.Debug("Resetting mixer volumes back to default", "AudioManager");
            MixerParameters[] parametersToReset =
            {
            MixerParameters.VolumeHeart, MixerParameters.VolumeLungs, MixerParameters.VolumeStethoscope,
            MixerParameters.VolumePatientBreathing, MixerParameters.VolumePatientSpeech
        };

            foreach (MixerParameters parameter in parametersToReset)
            {
                SetFloatParameter(parameter, 0f);
                UniTask.Yield();
            }
        }

        /// <summary>
        /// Responsible for creating and instantiating an AudioSource based in the AudioData send via audioName string
        /// </summary>
        /// <param name="audioName">Name of the audio to instantiate</param>
        /// <param name="audioVolume">Volume of the AudioSource</param>
        /// <param name="loopMode">Will we loop this audio?</param>
        /// <param name="audioPitch">Will we add pitch effects?</param>
        /// <param name="spatializeTransform">Transform reference for spatialized sound</param>
        /// <param name="audioSourceReference">Reference of the target AudioSource</param>
        /// <returns></returns>
        public AudioSource PlayAudio(string audioName, float audioVolume = 1f, bool loopMode = false, float audioPitch = 1f,
            Transform spatializeTransform = null, AudioSpatializationParameters spatializationParamters = null, AudioSource audioSourceReference = null)
        {
            if (scenarioManager.ScenarioStatus != GIGXR.Platform.Scenarios.Data.ScenarioStatus.Playing)
            {
                return null;
            }

            AudioData audioData = AudioDatabase.FindAudioDataByName(audioName);

            if (audioData == null)
            {
                return null;
            }

            //Check if is a looped audio, and them add it to the looping audio list instead of creating a new one
            //We only proceed if the actual looping audio is not playing yet
            if (loopingAudiosDictionary.ContainsKey(audioData.name))
            {
                Debug.Log("[AMBIENTAUDIO]I found a looped audio with the same name, exiting now");
                return null;
            }

            //Check if we need to dynamic instantiate an Audio Game Object or use an existing one (outside of pool)
            AudioSource audioSource;

            if (audioSourceReference == null)
            {
                audioSource = GetAvailableAudioFromThePool();
                audioSourcePool.Enqueue(audioSource);
            }
            else
            {
                audioSource = audioSourceReference;
            }

            if (audioSource != null)
            {
                audioSource.clip = audioData.audioClip;
                audioSource.outputAudioMixerGroup = AudioDatabase.FindAudioMixerByName(audioData.mixer);
            }

            //Parameters
            ChangeVolume(audioVolume, audioSource);
            ChangeLooping(loopMode, audioSource);
            ChangePitch(audioPitch, audioSource);
            ChangeSpatialize(spatializeTransform, spatializationParamters, audioSource);

            //Check for the audio speed boolean and see if we need to adjust the speed
            if (audioData.ChangeAudioSpeedBasedOnPitch && audioSource != null)
            {
                ChangePitch(audioData.pitch, audioSource);
                audioSource.outputAudioMixerGroup.audioMixer.SetFloat("Pitch" + audioData.mixer, 1 / audioData.pitch);
            }

            //Play the fresh created audio
            audioSource.Play();
            lastAudioCreated = audioSource;

            if (!loopMode)
            {
                return audioSource;
            }

            loopingAudiosDictionary.Add(audioData.name, audioSource);

            // else
            // {
            //     //Commented after the new audio pool system was created. Maybe will revisit this later
            //     DestroyAudioGameObject(audioSource);
            // }

            return audioSource;
        }

        public AudioSource ChangeAudioSourceByAudioData(string audioName, AudioSource audioSourceReference, bool skipVolume = false)
        {
            AudioSource audioSource = null;
            AudioData audioData = AudioDatabase.FindAudioDataByName(audioName);
            if (audioData == null)
            {
                return null;
            }

            //use an existing one
            if (audioSourceReference != null)
            {
                audioSource = audioSourceReference;
            }

            if (audioSource == null)
            {
                return null;
            }

            audioSource.clip = audioData.audioClip;
            audioSource.outputAudioMixerGroup = AudioDatabase.FindAudioMixerByName(audioData.mixer);

            //Parameters
            if (!skipVolume)
                ChangeVolume(audioData.volume, audioSource);

            //ChangeLooping(audioData.loop, audioSource);
            ChangePitch(audioData.pitch, audioSource);

            //Check for the audio speed boolean and see if we need to adjust the speed
            if (audioData.ChangeAudioSpeedBasedOnPitch)
            {
                audioSource.outputAudioMixerGroup.audioMixer.SetFloat("Pitch" + audioData.mixer, 1 / audioData.pitch);
            }

            return audioSource;
        }

        /// <summary>
        /// This is used to be able to change an audio clip of an audio data based in a condition pre-stablished in edit time
        /// </summary>
        /// <param name="audioKey"></param>
        /// <param name="variationCondition"></param>
        /// <param name="variationConditionValue"></param>
        public float ChangeAudioClipByConditionVariation(string audioKey, VariationCondition variationCondition,
            float variationConditionValue)
        {
            // get the audio data corresponding to the key
            AudioData audioData = AudioDatabase.FindAudioDataByName(audioKey);

            // select a variation based on the variation condition, adjust the audio data if needed, and return the condition value of the associated clip
            switch (variationCondition)
            {
                case VariationCondition.Random:
                    return SelectAudioClipByRandomVariation(audioData);

                case VariationCondition.RespiratoryRate:
                    // select the clip and apply to audioData
                    float bestConditionValue = SelectAudioClipByVariationCondition(audioData, variationCondition, variationConditionValue);

                    // adjust the pitch if needed. bestConditionValue is -1 if audio clip was unchanged.
                    if (bestConditionValue > 0)
                    {
                        audioData.pitch = variationConditionValue / bestConditionValue;
                    }

                    return bestConditionValue;

                default:
                    return SelectAudioClipByVariationCondition(audioData, variationCondition, variationConditionValue);
            }
        }

        private float SelectAudioClipByVariationCondition(AudioData audioData, VariationCondition variationCondition, float variationConditionValue)
        {
            AudioClipVariation bestCandidateToSwap = new AudioClipVariation();
            float currentBestCandidateDifference = float.MaxValue;
            foreach (AudioClipVariation audioClipVariation in audioData.audioClipVariation)
            {
                if (audioClipVariation.variationCondition != variationCondition)
                {
                    continue;
                }

                //If we are testing for the right condition
                float currentConditionTestValue = Mathf.Abs(variationConditionValue - audioClipVariation.variationConditionValue);

                if (!(currentConditionTestValue < currentBestCandidateDifference))
                {
                    continue;
                }

                //If we found out a better candidate to match the condition, we swap between them
                bestCandidateToSwap = audioClipVariation;
                currentBestCandidateDifference = currentConditionTestValue;
            }

            if (audioData.audioClip == bestCandidateToSwap.clipFromVariation || bestCandidateToSwap.clipFromVariation == null)
            {
                return -1;
            }

            // We only need to recalculate the piatch in the RespiratoryRate case. See ChangeAudioClipByConditionVariation for this.
            //audioData.pitch = variationConditionValue / bestCandidateToSwap.variationConditionValue;
            audioData.audioClip = bestCandidateToSwap.clipFromVariation;
            return bestCandidateToSwap.variationConditionValue;
        }

        private float SelectAudioClipByRandomVariation(AudioData audioData)
        {
            audioData.audioClip = audioData.audioClipVariation[random.Next(audioData.audioClipVariation.Count)].clipFromVariation;
            return 0f;
        }

        public void RemoveAndDestroyLoopingAudio(string audioName)
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return;
            }

            if (!loopingAudiosDictionary.ContainsKey(audioName))
            {
                return;
            }

            loopingAudiosDictionary[audioName].Stop();
            loopingAudiosDictionary.Remove(audioName);
            //GameObject audioToBeRemoved = loopingAudiosDictionary[audioName].gameObject;
            //GameObject.Destroy(audioToBeRemoved);
        }

        private void DestroyAudioGameObject(AudioSource audioSource)
        {
            waitAndDestroyHandler = new BackgroundWaitAndDestroyHandler();
            waitAndDestroyHandler.audioSource = audioSource;
            waitAndDestroyHandler.Enable();
        }

        public AudioSource StopAudio()
        {
            return null;
        }

        public void SetFloatParameter(MixerParameters parameter, float newVolume)
        {
            masterMixer.SetFloat(GetParameterName(parameter), newVolume);
        }

        public void GetFloatParameter(MixerParameters parameter, out float value)
        {
            masterMixer.GetFloat(GetParameterName(parameter), out value);
        }

        /// <summary>
        /// Stop all playing audios in the pool, including looping sounds (useful for when changing stages)
        /// </summary>
        public void StopAllSounds()
        {
            foreach (AudioSource audioSource in audioSourcePool)
            {
                audioSource.Stop();
            }
        }


        /// <summary>
        /// Stop all playing audios in the pool, but let the looping sounds running
        /// </summary>
        public void StopPlayingSoundsButNotLoopingSounds()
        {
            foreach (AudioSource audioSource in audioSourcePool)
            {
                if (!audioSource.loop)
                    audioSource.Stop();
            }
        }

        /// <summary>
        /// List of methods that can be chained together to change the Audio parameters
        /// </summary>

        #region Parameter Methods

        public float GetCurrentVolume(AudioSource audioReceived = null)
        {
            if (audioReceived == null)
                audioReceived = lastAudioCreated;

            return audioReceived.volume;
        }

        public AudioSource ChangeVolume(float audioVolume, AudioSource audioReceived = null)
        {
            if (audioReceived == null)
                audioReceived = lastAudioCreated;

            audioReceived.volume = audioVolume;

            return audioReceived;
        }

        public AudioSource ChangeLooping(bool loopMode, AudioSource audioReceived = null)
        {
            if (audioReceived == null)
                audioReceived = lastAudioCreated;

            audioReceived.loop = loopMode;

            return audioReceived;
        }

        public AudioSource ChangePitch(float audioPitch, AudioSource audioReceived = null)
        {
            if (audioReceived == null)
                audioReceived = lastAudioCreated;

            audioReceived.pitch = audioPitch;

            return audioReceived;
        }

        //Override in audio pitch
        public AudioData ChangePitch(float audioPitch, AudioData audioReceived)
        {
            audioReceived.pitch = audioPitch;

            return audioReceived;
        }

        public AudioSource ChangeSpatialize(Transform spatializeTransform, AudioSpatializationParameters spatializationParameters = null, AudioSource audioReceived = null)
        {
            if (audioReceived == null)
                audioReceived = lastAudioCreated;

            if (spatializeTransform != null)
            {
                audioReceived.spatialize = true;
                audioReceived.spatialBlend = 1f;

                // if spatialization parameters were supplied, apply them
                if (spatializationParameters != null)
                {
                    ApplySpatializationParameters(audioReceived, spatializationParameters);
                }
                // if spatialization parameters were not supplied and the audio is from the pool (as opposed to provided by the caller), revert it to default spatialization parameters
                else if (IsAudioSourceFromPool(audioReceived))
                {
                    ApplySpatializationParameters(audioReceived, AudioSpatializationParameters.Default);
                }

                // if the audio source was not from the pool, but was passed in by the caller, and the caller provided no spatialization parameters, then let the caller manage the parameters.

                audioReceived.transform.position = spatializeTransform.position;
            }
            else
            {
                audioReceived.spatialize = false;
                audioReceived.spatialBlend = 0f;
            }

            return audioReceived;
        }

        private void ApplySpatializationParameters(AudioSource audioReceived, AudioSpatializationParameters spatializationParameters)
        {
            audioReceived.minDistance = spatializationParameters.MinDistance;
            audioReceived.maxDistance = spatializationParameters.MaxDistance;
            audioReceived.rolloffMode = spatializationParameters.RolloffMode;
            audioReceived.dopplerLevel = spatializationParameters.DopplerLevel;
            audioReceived.spread = spatializationParameters.Spread;
        }

        private bool IsAudioSourceFromPool(AudioSource audioReceived)
        {
            return audioPoolMembers.Contains(audioReceived);
        }

        #endregion

        #region Sound effects methods

        [Obsolete("Pass the effect as a MixerParameters instead of string")]
        public void AddMixerEffect(AudioMixerType mixerName, string effectName, float amount)
        {
            AudioMixer mixer = AudioDatabase.FindAudioMixerByName(mixerName).audioMixer;
            mixer.SetFloat(effectName, amount);
        }

        public void AddMixerEffect(AudioMixerType mixerName, MixerParameters parameter, float amount)
        {
            AudioMixer mixer = AudioDatabase.FindAudioMixerByName(mixerName).audioMixer;
            mixer.SetFloat(GetParameterName(parameter), amount);
        }

        #endregion

        //For the future, to be able to control audio states
        public event EventHandler<EventArgs> AudioPlayed;
        public event EventHandler<EventArgs> AudioPaused;
        public event EventHandler<EventArgs> AudioFinished;
    }
}