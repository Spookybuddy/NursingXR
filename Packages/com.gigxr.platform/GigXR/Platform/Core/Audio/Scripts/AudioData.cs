using GIGXR.Platform.Utilities.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GIGXR.Platform.Core.Audio
{
    /// <summary>
    /// Enum that allows to easily find which Mixer that sound is part of
    /// </summary>
    [Serializable]
    public enum AudioMixerType
    {
        Master = 0,
        Stethoscope = 1,
        PatientBreathing = 2,
        Masks = 3,
        UI = 4,
        Lungs = 5,
        Heart = 6,
        PatientSpeech = 7,
        Devices = 8,
        Ambience = 9
    }

    #region Dictionary properties

    /// <summary>
    /// This is a custom Dictionary created to hold the keys and values for each collection of audios from each Scenario
    /// </summary>
    [Serializable]
    public class AudioDataDictionary : SerializableDictionary<string, AudioData>
    {

    }

    #endregion

    /// <summary>
    /// For now, we have an scriptable object with all the populated fields for this class
    /// In the future, could be good to have all these files populated dynamic via JSON and also finding the references via serialization/resources
    /// </summary>
    [CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/AudioData", fileName = "AudioData")]
    [Serializable]
    public class AudioData : ScriptableObject
    {

        public AudioClip audioClip;
        [Header("Optional - Variation for audio clip")] public List<AudioClipVariation> audioClipVariation;
        public string name;
        public float volume;
        public float pitch;
        public bool loop;
        public AudioMixerType mixer;
        public float animationLength;
        public bool ChangeAudioSpeedBasedOnPitch;

    }
}