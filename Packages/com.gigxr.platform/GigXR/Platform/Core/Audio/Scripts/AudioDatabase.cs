using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Logger = GIGXR.Platform.Utilities.Logger;

namespace GIGXR.Platform.Core.Audio
{
    /// <summary>
    /// Simple class to store all the audios registed in the app as they were part of a database, with search options as well
    /// </summary>
    [CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/AudioDatabase", fileName = "AudioDatabase")]
    [Serializable]
    public class AudioDatabase : ScriptableObject
    {
        public AudioScenarioCollectionDictionary audioScenarioCollections;

        public AnimationEventsCollectionDictionary animationEventsCollections;

        [SerializeField] private List<AudioMixerGroup> audioMixerList;

        private string currentScenarioName;

        /// <summary>
        /// Stores the scenario name as the key that'll be used for searching the dictionaries of each scenario
        /// </summary>
        /// <param name="currentScenario"></param>
        public void InitializeAudioDatabase(string currentScenario)
        {
            currentScenarioName = currentScenario;
        }

        /// <summary>
        /// Clear all events when swapping scenarios (needed to not have animation events duplicated)
        /// </summary>
        /// <param name="patientAnimator"></param>
        public void ClearAnimationEvents(Animator patientAnimator)
        {
            //Clear previous possible events when swapping scenarios
            List<AnimationClip> animationClipsList = patientAnimator.runtimeAnimatorController.animationClips.ToList();
            foreach (AnimationClip animationClip in animationClipsList)
            {
                AnimationEvent[] clearEvents = Array.Empty<AnimationEvent>();
                animationClip.events = clearEvents;
            }
        }

        /// <summary>
        /// Creates all the animation events in their respective times during runtime
        /// </summary>
        /// <param name="patientAnimator"></param>
        public void InitializeAnimationEvents(Animator patientAnimator)
        {
            if (!animationEventsCollections.ContainsKey(currentScenarioName))
            {
                return;
            }

            //Loop through all events registered
            foreach (AnimationEventData animationEventData in animationEventsCollections[currentScenarioName].animationEventDataList)
            {
                List<AnimationClip> animationClipsList = patientAnimator.runtimeAnimatorController.animationClips.ToList();
                foreach (AnimationClip animationClip in animationClipsList.Where
                             (animationClip => animationClip.name == animationEventData.animationClipName))
                {
                    Logger.Debug
                        ($"Adding animation event + {animationEventData.methodToCall} in clip {animationClip.name}", "AudioDatabase");
                    animationClip.AddEvent(animationEventData.GetAnimationEvent());
                }
            }
        }

        /// <summary>
        /// Creates all the animation events in their respective times during runtime for a specific stage
        /// </summary>
        /// <param name="patientAnimator"></param>
        /// <param name="currentStageTitle"></param>
        /// <param name="currentStageId"></param>
        public void InitializeAnimationEventsForSpecificStage(Animator patientAnimator, string currentStageTitle,
            string currentStageId)
        {
            if (!animationEventsCollections.ContainsKey(currentScenarioName))
            {
                return;
            }

            Logger.Debug("I found some animation events for current scenario", "AudioDatabase");

            ClearAnimationEvents(patientAnimator);

            //Loop through all events registered
            foreach (AnimationEventData animationEventData in animationEventsCollections[currentScenarioName].animationEventDataList)
            {
                if (animationEventData.stageToPlay != currentStageTitle && animationEventData.stageToPlayId != currentStageId)
                {
                    continue;
                }

                Logger.Debug("I found an animation events for current stage", "AudioDatabase");
                string lastAnimationAdded = "";


                List<AnimationClip> animationClipsList = patientAnimator.runtimeAnimatorController.animationClips.ToList();
                foreach (AnimationClip animationClip in animationClipsList.Where
                         (
                             animationClip => animationClip.name == animationEventData.animationClipName &&
                                              lastAnimationAdded != animationClip.name
                         ))
                {
                    Logger.Debug
                    (
                        $"Adding animation event + {animationEventData.methodToCall} in clip {animationClip.name} for stage {currentStageTitle}",
                        "AudioDatabase"
                    );
                    animationClip.AddEvent(animationEventData.GetAnimationEvent());
                    lastAnimationAdded = animationClip.name;
                }
            }
        }

        public List<AudioData> GetAudioScenarioCollection()
        {
            return audioScenarioCollections.ContainsKey
                (currentScenarioName)
                ? audioScenarioCollections[currentScenarioName].AudioDataDictionary
                : null;
        }

        public AudioData FindAudioDataByName(string nameReceived)
        {
            if (string.IsNullOrEmpty(currentScenarioName))
            {
                Logger.Debug("Audio lookup failed; scenario name not set.", "AudioDatabase");
                return null;
            }

            if (audioScenarioCollections.ContainsKey(currentScenarioName))
            {
                return audioScenarioCollections[currentScenarioName]
                    .AudioDataDictionary.FirstOrDefault(item => item.name == nameReceived);
            }

            Logger.Debug($"Couldn't find audio with key {nameReceived} in collection {currentScenarioName}", "AudioDatabase");
            return null;
        }

        public AudioMixerGroup FindAudioMixerByName(AudioMixerType audioDataMixer)
        {
            string audioDataMixerStr = audioDataMixer.ToString();
            return audioMixerList.FirstOrDefault(item => item.name == audioDataMixerStr);
        }

        //TODO: Add methods like: find audio by name, find audio by duration, find audio by category, etc
    }
}