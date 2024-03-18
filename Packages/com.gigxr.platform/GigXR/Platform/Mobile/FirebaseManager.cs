using System.Reflection;
using UnityEngine;

using Firebase;
using Firebase.DynamicLinks;
using Firebase.Messaging;

using JetBrains.Annotations;

using GIGXR.GMS.Clients;
using GIGXR.Platform.Interfaces;

namespace GIGXR.Platform.Mobile
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.AppEvents;
    using Platform.AppEvents.Events.Authentication;

    public class FirebaseManager : IFirebaseManager
    {   
        // --- Events:
        public event IFirebaseManager.DynamicLinkReceivedEventHandler DynamicLinkReceived;

        // --- Private Variables:

        private FirebaseApp firebaseApp;

        private GmsApiClient GmsApiClient { get; set; }

        private AppEventBus EventBus { get; set; }

        private MobileProfileScriptableObject MobileProfile { get; set; }

        // --- Public Properties:
        public bool IsReady { get; private set; }

        [CanBeNull]
        public string DeviceToken
        {
            get => PlayerPrefs.GetString("gigxr-firebase-device-token");
            set => PlayerPrefs.SetString("gigxr-firebase-device-token", value);
        }

        // --- Unity Methods:
        
        public FirebaseManager(GmsApiClient gmsApiClient, AppEventBus appEventBus, MobileProfileScriptableObject mobileProfile)
        {
            GmsApiClient = gmsApiClient;
            EventBus = appEventBus;
            MobileProfile = mobileProfile;

            if (!MobileProfile.ShouldInitFirebase)
            {
                IsReady = true;
                return;
            }

            EventBus.Subscribe<AuthenticatedUserEvent>(OnAuthenticatedUserEvent);

            FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask().ContinueWith(async dependencyStatus =>
            {
                await UniTask.SwitchToMainThread();

                if (dependencyStatus == DependencyStatus.Available)
                {
                    firebaseApp = FirebaseApp.DefaultInstance;

#if UNITY_ANDROID
                    // Android does not require confirmation for messaging so we can initialize right away.
                    TryEnableMessaging();
#elif UNITY_IOS
                    // On iOS as soon as we add the events it will trigger asking the user if they want to enable
                    // notifications. We want to defer this until they have gone through the first time experience.
                    if (PlayerPrefs.HasKey("gigxr-skip-first-time-experience"))
                    {
                        TryEnableMessaging();
                    }
#endif

                    TryEnableDynamicLinks();

                    IsReady = true;
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        // --- Event Handlers:

        /// <summary>
        /// Event invoked when a firebase registration token is received.
        /// </summary>
        public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log("Received Registration Token: " + token.Token);

            DeviceToken = token.Token;
        }

        public void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Debug.Log("Received a new message from: " + args.Message.From);

            Debug.Log("received message type:" + args.Message.MessageType);
            if (args.Message.NotificationOpened && args.Message.Data.ContainsKey("Path"))
            {
                Debug.Log($"Received: Deep linking from notification to: {args.Message.Data["Path"]}");
                CloudLogger.LogInformation($"Deep linking from notification to: {args.Message.Data["Path"]}");
                DynamicLinkReceived?.Invoke(args.Message.Data["Path"]);
            }
        }

        /// <summary>
        /// Event handler for received dynamic links.
        /// </summary>
        private void OnDynamicLinkReceived(object sender, ReceivedDynamicLinkEventArgs args)
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());

            Debug.Log($"Received dynamic link: {args.ReceivedDynamicLink.Url.OriginalString}");
            CloudLogger.LogInformation($"Received dynamic link: {args.ReceivedDynamicLink.Url.OriginalString}");

            DynamicLinkReceived?.Invoke(args.ReceivedDynamicLink.Url.PathAndQuery);

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        // --- Public Methods:

        /// <summary>
        /// If cloud messaging is enabled in the <see cref="ProfileManager"/>,
        /// event handlers for received tokens and messages are registered.
        /// </summary>
        /// <returns>
        /// <c>true</c> if cloud messaging is enabled. <c>false</c> otherwise.
        /// </returns>
        public bool TryEnableMessaging()
        {
            if (!MobileProfile.EnableCloudMessaging)
            {
                return false;
            }

            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            return true;
        }

        /// <summary>
        /// If dynamic links are enabled in the <see cref="ProfileManager"/>,
        /// the event handler for received dynamic links is registered here.
        /// </summary>
        /// <returns>
        /// <c>true</c> if dynamic links are enabled. <c>false</c> otherwise.
        /// </returns>
        public bool TryEnableDynamicLinks()
        {
            if (!MobileProfile.EnableDynamicLinks)
            {
                return false;
            }

            DynamicLinks.DynamicLinkReceived += OnDynamicLinkReceived;

            return true;
        }

        /// <summary>
        /// Unregister Firebase token with the API so this device will no longer receive notifications
        /// for the currently authenticated user.
        /// 
        /// TODO This endpoint doesn't exist in GMS anymore, not sure if we should keep this around
        /// </summary>
        /*public async void DeleteFirebaseToken()
        {
            await GmsApiClient.AccountsApi.DeleteFirebaseRegistrationTokenAsync(DeviceToken);
        }*/

        /// <summary>
        /// When a user successfully authenticates, update GMS with the device's
        /// firebase registration token so notifications for the authenticated user
        /// are sent to this device.
        /// </summary>
        private async void OnAuthenticatedUserEvent(AuthenticatedUserEvent @event)
        {
            if (@event.AuthenticationStatus == Interfaces.IAuthenticationManager.AuthenticationStatus.Success)
            {
                await GmsApiClient.AccountsApi.UpdateFirebaseRegistrationTokenAsync(DeviceToken);
            }
        }
    }
}