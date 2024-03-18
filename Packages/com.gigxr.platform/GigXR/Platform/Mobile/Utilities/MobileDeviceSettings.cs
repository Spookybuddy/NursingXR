using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using System;

namespace GIGXR.Platform.Mobile.Utilities
{
    /// <summary>
    /// Monobehavior to initialize mobile device settings on Awake.
    /// Responsible for intializing mobile-device-specific settings
    /// and requesting Android permissions.
    /// </summary>
    public class MobileDeviceSettings
    {
        /// <summary>
        /// Initialize mobile device settings. Turns off sleep timout.
        /// Requests camera and microphone permissions on Android.
        /// These device requests will be made automatically when
        /// the corresponding device is needed on iOS.
        /// </summary>
        public static void Initialize()
        {
#if PLATFORM_ANDROID
            RequestAndroidPermissions();

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

#if PLATFORM_ANDROID
        private static void RequestAndroidPermissions()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                PermissionCallbacks callbacks = new PermissionCallbacks();

                Action<string> cameraRequest = (string s) =>
                {
                    if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                    {
                        Permission.RequestUserPermission(Permission.Camera);
                    }
                };

                callbacks.PermissionGranted += cameraRequest;
                callbacks.PermissionDenied += cameraRequest;
                callbacks.PermissionDeniedAndDontAskAgain += cameraRequest;

                Permission.RequestUserPermission(Permission.Microphone, callbacks);
            }
            else if(!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
        }
#endif
    }
}