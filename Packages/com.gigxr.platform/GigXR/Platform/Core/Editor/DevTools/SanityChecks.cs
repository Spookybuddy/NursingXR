// #if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SanityChecks
{
#if UNITY_STANDALONE_WIN

[InitializeOnLoad]
public static class CheckBuildPlatform
{
    static CheckBuildPlatform()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        Debug.LogError("You are in PC/Standalone mode which is not supported.");
    }
}
    
#endif
}
// #endif
