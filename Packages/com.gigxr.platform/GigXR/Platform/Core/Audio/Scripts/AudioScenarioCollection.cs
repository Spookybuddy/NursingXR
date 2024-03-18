using GIGXR.Platform.Utilities.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Core.Audio
{
    #region Dictionary properties

    /// <summary>
    /// This is a custom Dictionary created to hold the keys and values for each collection of audios from each Scenario
    /// </summary>
    [Serializable]
    public class AudioScenarioCollectionDictionary : SerializableDictionary<string, AudioScenarioCollection>
    {

    }

    #endregion

    /// <summary>
    /// Simple class to store all the audios registed in the app as they were part of a database, with search options as well
    /// </summary>
    [CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/AudioScenarioCollection", fileName = "AudioScenarioCollection")]
    [Serializable]
    public class AudioScenarioCollection : ScriptableObject
    {

        public List<AudioData> AudioDataDictionary;

    }
}
