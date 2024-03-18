using UnityEngine;

namespace GIGXR.Platform.HMD
{
    /// <summary>
    /// Allows a ScriptableObject to define what tabs are available in the hand menu.
    /// </summary>
    [CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/New Hand Menu Tab")]
    public class HandMenuTabScriptableObject : ScriptableObject
    {
        public string tabName;

        public bool useCalibration;
    }
}