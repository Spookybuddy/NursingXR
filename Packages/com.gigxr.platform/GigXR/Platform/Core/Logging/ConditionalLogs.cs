using UnityEngine;

namespace GIGXR.Platform.Core.Logging
{
    using Microsoft.MixedReality.Toolkit.Utilities;
    using System;

    public class ConditionalLogs : MonoBehaviour
    {
        [SerializeField]
        [Header("Sets the logging level for the build.")]
        private DebugUtilities.LoggingLevel loggingLevel; 
        
        private void Awake()
        {
            DebugUtilities.LogLevel = loggingLevel;
            
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
            if (loggingLevel == DebugUtilities.LoggingLevel.None)
            {
                Debug.LogError("disabling logs other than errors...");
                Debug.unityLogger.filterLogType = LogType.Error; // will show errors and exceptions
                Debug.LogWarning("test");
                Debug.LogError("test");
                Debug.LogException(new Exception("test"));
            }
#endif
        }
    }
}
