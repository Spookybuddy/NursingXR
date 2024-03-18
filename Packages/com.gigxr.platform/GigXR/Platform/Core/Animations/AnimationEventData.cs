using System;
using UnityEngine;

/// <summary>
/// For now, we have an scriptable object with all the populated fields for this class
/// This contains all the data for a given AnimationEvent to be added at some point for any given animation clip
/// </summary>
[CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/AnimationEvent", fileName = "AnimationEvent")]
[Serializable]
public class AnimationEventData : ScriptableObject
{

    public string stageToPlay;
    public string stageToPlayId;
    public string animationClipName;
    public string methodToCall;
    public string animationEventArgs;
    public float eventTime;

    public AnimationEvent GetAnimationEvent()
    {

        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.time = eventTime;
        animationEvent.stringParameter = animationEventArgs;
        animationEvent.functionName = methodToCall;

        return animationEvent;

    }

}
