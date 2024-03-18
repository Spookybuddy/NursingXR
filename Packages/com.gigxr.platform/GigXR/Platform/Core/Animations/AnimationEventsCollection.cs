using GIGXR.Platform.Utilities.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

#region Dictionary properties

/// <summary>
/// This is a custom Dictionary created to hold the keys and values for each collection of audios from each Scenario
/// </summary>
[Serializable]
public class AnimationEventsCollectionDictionary : SerializableDictionary<string, AnimationEventsCollection> 
{
    
}

#endregion

/// <summary>
/// Simple class to store all the audios registed in the app as they were part of a database, with search options as well
/// </summary>
[CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/AnimationEventsCollection", fileName = "AnimationEventsCollection")]
[Serializable]
public class AnimationEventsCollection : ScriptableObject
{

    public List<AnimationEventData> animationEventDataList;


}