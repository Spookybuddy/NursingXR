using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGazeHintProvider
{
    Transform AttachedTransform { get; }

    Transform[] GazeHintList { get; }

    Transform LastRandomTransform { get; }

    Transform GetRandomTarget();
}

public class LookAtHintProvider : MonoBehaviour, IGazeHintProvider
{
    public Transform[] gazeHintList;

    public Transform[] GazeHintList => gazeHintList;

    public Transform AttachedTransform => transform;

    public Transform LastRandomTransform => lastRandomTransform;

    private Transform lastRandomTransform;

    public Transform GetRandomTarget()
    {
        if (gazeHintList.Length == 0)
            return transform;

        lastRandomTransform = gazeHintList[Random.Range(0, gazeHintList.Length - 1)];

        return lastRandomTransform; 
    }
}
