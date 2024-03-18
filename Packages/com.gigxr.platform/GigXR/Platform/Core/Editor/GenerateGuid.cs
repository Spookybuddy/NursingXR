using System;
using UnityEditor;
using UnityEngine;

public class GenerateGuid : Editor
{
    /// <summary>
    /// Generates and copies a new Guid string into the Clipboard.
    /// </summary>
    [MenuItem("GIGXR/Utilities/Generate Random Guid and Copy to Clipboard")]
    public static void GenerateGuidAndCopyToClipboard()
    {
        Guid newGuid = Guid.NewGuid();
        GUIUtility.systemCopyBuffer = newGuid.ToString();
    }
}