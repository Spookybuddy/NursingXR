using TMPro;
using UnityEngine;
using GIGXR.Platform.Managers;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.HMD.Interfaces;

namespace GIGXR.Platform.HMD.UI
{
    using Platform.AppEvents.Events.Authentication;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.AppEvents.Events.UI.ButtonEvents;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.AppEvents.Events.Authentication;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Networking;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using GIGXR.Platform.HMD.AppEvents.Events;
    using GIGXR.Platform.UI;
    using System.Linq;
    using System.Collections.Generic;
    using GIGXR.Platform.Core;
    using Cysharp.Threading.Tasks;
    using UnityEngine.UI;

    /// <summary>
    /// HMD specific screen for displaying the inputs for logging into GMS. This screen provides an option
    /// to login via QR code or via a login button after the user types their username and password.
    /// 
    /// TMP_InputField are used for inputs so they will automatically bring up the system keyboard.
    /// </summary>
    public class AuthenticationScreen : BaseScreenObject
    {
        [Header("UI references for email & password fields:")]
        [SerializeField]
        private TMP_InputField emailInput;

        [SerializeField]
        private TMP_InputField passwordInput;

        [SerializeField]
        private TextMeshProUGUI appTitleField;

        [SerializeField]
        private Image appIconField;

        private TextMeshProUGUI qrCalibrationText;

        private TextMeshProUGUI versionTextDisplay;

        [SerializeField]
        private Vector3 versionLocation = new Vector3(0.0f, -0.125f, 0.0f);

        private IAuthenticationManager AuthenticationManager { get; set; }

        private INetworkManager NetworkManager { get; set; }

        private IQrCodeManager QrCodeManager { get; set; }

        private ProfileManager ProfileManager { get; set; }

        private IBuilderManager BuilderManager { get; set; }

        public override ScreenType ScreenObjectType => ScreenType.Authentication;

        private UIPlacementData promptPlacement;

    [InjectDependencies]
        public void Construct
        (
            IAuthenticationManager authManager,
            INetworkManager networkManager,
            IQrCodeManager qrCodeManager,
            ProfileManager profileManager,
            IBuilderManager builderManager
        )
        {
            AuthenticationManager = authManager;
            NetworkManager = networkManager;
            QrCodeManager = qrCodeManager;
            ProfileManager = profileManager;
            BuilderManager = builderManager;

            if (appTitleField != null)
            {
                string appName = ProfileManager.appDetailsProfile.appDetailsScriptableObject
                    .appName;

                appTitleField.SetText(appName);
            }

            if(appIconField != null)
            {
                appIconField.sprite = ProfileManager.appDetailsProfile.appDetailsScriptableObject.AppIcon;
            }

            AdjustAfterEnvironmentSet();
        }

        public void AdjustAfterEnvironmentSet()
        {
            // Add the current version string underneath the screen object
            var versionString = $"Version {ProfileManager.appDetailsProfile.appDetailsScriptableObject.VersionString}";

            // Only when in a QA environment, add that addition to the string so it's obvious in app
            if (ProfileManager.authenticationProfile.TargetEnvironmentalDetails.IsQAEnvironment)
            {
                versionString += "-QA";
            }

            // Only build the text display once, it will set the initial value when created
            if(versionTextDisplay == null)
            {
                var versionDisplayObject = BuilderManager.BuildText(text: versionString,
                textBoxSize: new Vector2(0.1f, 0.01f),
                fontSize: 0.0085f);

                versionDisplayObject.transform.SetParent(RootScreenTransform, false);
                versionDisplayObject.transform.localPosition = versionLocation;

                versionTextDisplay = versionDisplayObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                versionTextDisplay.text = versionString;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            promptPlacement = new UIPlacementData { HostTransform = RootScreenTransform.transform };

            // A bit of a mouthful, but we only need to get the TextMesh component to set some text and wanted to avoid extra classes
            qrCalibrationText = GetComponentsInChildren<SubScreenObject>(true)
                .Where((currentSubScreen) => currentSubScreen.SubState == SubScreenState.QRLogin)
                .First()
                .GetComponentInChildren<TextMeshProUGUI>(true);

            base.Initialize();
        }

        protected void OnApplicationFocus(bool isFocused)
        {
            // When the app is in focus, we want the JWT to stay active while the users is using it,
            // otherwise we do not keep it active and will log the user out if they attempt to use
            // the app after the set amount of time
            AuthenticationManager?.SetJWTHandlerActivity(isFocused);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventBus.Unsubscribe<AuthenticatedUserEvent>(OnAuthenticatedUserEvent);

            EventBus.Unsubscribe<SuccessfulAuthenticationFinishScreenEvent>
                (OnSuccessfulAuthenticationFinishScreenEvent);

            EventBus.Unsubscribe<FailedAuthenticationFinishScreenEvent>
                (OnFailedAuthenticationFinishScreenEvent);

            EventBus.Unsubscribe<FinishedLogOutEvent>(OnFinishedLogOutEvent);
            EventBus.Unsubscribe<StopQrTrackingEvent>(OnStopQrTrackingEvent);
            EventBus.Unsubscribe<SetQrFeedbackTextEvent>(OnSetQrFeedbackTextEvent);
            EventBus.Unsubscribe<QrDeniedFeedbackEvent>(OnQrDeniedFeedbackEventAsync);

            // The token expiration is only useful if the app is left running and we need to log the user
            // out from inactivity, if they quit the app, we don't need to worry about this
            // Note: Do not use OnApplicationQuit as it is unreliable for Mobile devices
            PlayerPrefs.DeleteKey("tokenExpiration");
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<AuthenticatedUserEvent>(OnAuthenticatedUserEvent);

            EventBus.Subscribe<SuccessfulAuthenticationFinishScreenEvent>
                (OnSuccessfulAuthenticationFinishScreenEvent);

            EventBus.Subscribe<FailedAuthenticationFinishScreenEvent>
                (OnFailedAuthenticationFinishScreenEvent);

            EventBus.Subscribe<FinishedLogOutEvent>(OnFinishedLogOutEvent);
            EventBus.Subscribe<StopQrTrackingEvent>(OnStopQrTrackingEvent);
            EventBus.Subscribe<SetQrFeedbackTextEvent>(OnSetQrFeedbackTextEvent);
            EventBus.Subscribe<QrDeniedFeedbackEvent>(OnQrDeniedFeedbackEventAsync);

            // The AuthenticationScreen is the first screen that is up, not sure where the best place to put this
            // initial behavior, but we want the login screen to follow the user's gaze
            uiEventBus.Publish(new SettingScreenFollowBehaviorEvent(ScreenObjectType, true));
        }

        private void OnAuthenticatedUserEvent(AuthenticatedUserEvent @event)
        {
            uiEventBus.Publish(new SettingGlobalButtonStateEvent(true, false));

            if (@event.AuthenticationStatus == IAuthenticationManager.AuthenticationStatus.Success)
            {
                EventBus.Publish(new SuccessfulAuthenticationFinishScreenEvent());
            }
            else if(@event.AuthenticationStatus != IAuthenticationManager.AuthenticationStatus.FailedUnknownReason)
            {
                EventBus.Publish(new FailedAuthenticationFinishScreenEvent(@event.Message));
            }
        }

        private void OnSuccessfulAuthenticationFinishScreenEvent
            (SuccessfulAuthenticationFinishScreenEvent @event)
        {
            // Bring down authentication screen when finished
            uiEventBus.Publish(new SettingScreenVisibilityEvent(ScreenObjectType, false));
            
            // Because these are screens, we want them to appear flat from this point forward, but we still need them
            // to point towards the user, so wipe out the x axis so it's flat vertically
            var flatRotation = Quaternion.Euler(0,
                                                RootScreenObject.transform.rotation.eulerAngles.y,
                                                RootScreenObject.transform.rotation.eulerAngles.z);

            uiEventBus.Publish
            (
                new ScreenPlacementHintEvent
                (
                    ScreenType.Calibration, 
                    RootScreenObject.transform.position,
                    flatRotation,
                    true
                )
            );

            ResetAuthenticationScreenUI();
        }

        private void OnFailedAuthenticationFinishScreenEvent(FailedAuthenticationFinishScreenEvent @event)
        {
            DebugUtilities.Log("AuthenticationScreen: OnFailedAuthenticationFinishScreenEvent");

            string message = string.IsNullOrEmpty(@event.Message) ? "Login Failed" : @event.Message;

            EventBus.Publish(new ShowTimedPromptEvent(message, null, promptPlacement));

            ResetAuthenticationScreenUI();
        }

        private void OnStopQrTrackingEvent(StopQrTrackingEvent @event)
        {
            if (@event.ReturnToAuthScreen)
            {
                // Bring up the authentication window again when the user logs out
                uiEventBus.Publish(new SettingScreenVisibilityEvent(ScreenObjectType, true));
            }
        }

        private void OnSetQrFeedbackTextEvent(SetQrFeedbackTextEvent @event)
        {
            if (qrCalibrationText != null)
            {
                qrCalibrationText.text = @event.FeedbackText;
            }
        }

        private void OnQrDeniedFeedbackEventAsync(QrDeniedFeedbackEvent @event)
        {
            // TODO NH I could not get the Windows.System.Launcher.LaunchUriAsync command to actually compile but it would be great to get this working
            /*var gotoSettingsButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = "Go to Settings",
                    onPressAction = async () =>
                    {
                        string uriToLaunch = @"ms-settings:privacy-webcam"; //Settings URI
                        System.Uri uri = new System.Uri(uriToLaunch);
                        await Windows.System.Launcher.LaunchUriAsync(uri);
                    }
                }
            };*/

            // TODO Refactor out into it's own string table
            EventBus.Publish
            (
                new ShowTimedPromptEvent
                    (
                        $"Unable to scan QR Code\n\nQR login requires access to HoloLens camera and gaze.Please provide {ProfileManager.appDetailsProfile.appDetailsScriptableObject.appName} access by visiting the privacy section within HoloLens settings. Then attempt QR login again.",
                        null,
                        PromptManager.WindowStates.Wide,
                        RootScreenTransform
                    )
            );
        }

        private void ResetAuthenticationScreenUI()
        {
            ResetSavedCredentials();
            EventBus.Publish(new HideProgressIndicatorEvent());
        }

        protected override void OnStartLogOutEvent(StartLogOutEvent @event)
        {
            if (!string.IsNullOrEmpty(@event.LogoutMessage))
            {
                EventBus.Publish(new ShowTimedPromptEvent(@event.LogoutMessage, null, promptPlacement));
            }
        }

        private void OnFinishedLogOutEvent(FinishedLogOutEvent @event)
        {
            // Bring up the authentication window again when the user logs out
            uiEventBus.Publish(new SettingScreenVisibilityEvent(ScreenObjectType, true));

            ResetAuthenticationScreenUI();
        }

        private void ResetSavedCredentials()
        {
            emailInput.text = "";
            passwordInput.text = "";
        }

        /// <summary>
        /// Instructs the Authentication Manager to authenticate with the cached username and password.
        /// </summary>
        public void Authenticate()
        {
            // todo 
            // need to recover button state on reset
            // or do a timed button thing
            // uiEventBus.Publish(new SettingGlobalButtonStateEvent(false, true));

            string cachedEmail = emailInput.text;
            string cachedPassword = passwordInput.text;

            // Use test details in the editor
#if UNITY_EDITOR
            var testEmail = EditorAuthenticationProfile.GetTestCredentials()?.Email;
            var testPassword = EditorAuthenticationProfile.GetTestCredentials()?.Password;

            cachedEmail = !string.IsNullOrEmpty(testEmail) ? testEmail : cachedEmail;
            cachedPassword = !string.IsNullOrEmpty(testPassword) ? testPassword : cachedPassword;

            DebugUtilities.LogVerbose($"Authenticating with {cachedEmail} and {cachedPassword}");
#endif

            // TODO - validate email format.
            if (cachedEmail == "")
            {
                EventBus.Publish
                    (
                        new ShowTimedPromptEvent
                            (
                                $"Please enter a valid email.",
                                null,
                                PromptManager.WindowStates.Wide,
                                RootScreenTransform,
                                2000
                            )
                    );
            }
            else if ((cachedPassword == "") &&
                     (cachedEmail != ""))
            {
                EventBus.Publish
                    (
                        new ShowTimedPromptEvent
                            (
                                $"Please enter a password.",
                                null,
                                PromptManager.WindowStates.Wide,
                                RootScreenTransform,
                                2000
                            )
                    );
            }
            else
            {
                EventBus.Publish(new ShowProgressIndicatorEvent());
                EventBus.Publish(new BeginAuthenticatingEvent(cachedEmail, cachedPassword));
            }
        }

        /// <summary>
        /// Opens the Terms and Conditions page for the application in the device's browser.
        /// </summary>
        public void OpenTermsURL()
        {
            Application.OpenURL(ProfileManager.authenticationProfile.TermsAndConditionsURL);
        }

        public void OpenCreditsURL()
        {
            Application.OpenURL(ProfileManager.authenticationProfile.CreditsURL);
        }

        /// <summary>
        /// Handle showing the visual prompt if the user wants to switch GMS environments on the Authentication screen.
        /// </summary>
        public void PromptEnvironmentSwitch(EnvironmentDetailsScriptableObject environmentDetails)
        {
            var confirmCancelButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = "Yes",
                    onPressAction = () =>
                    {
                        ProfileManager.authenticationProfile.SetTargetGMS(environmentDetails);

                        AuthenticationManager.UpdateEnvironment();

                        AdjustAfterEnvironmentSet();

                        // This causes the Auth screen to return
                        EventBus.Publish(new FinishedLogOutEvent());
                    }
                },
                new ButtonPromptInfo(){
                    buttonText = "No",
                    onPressAction= () =>
                    {
                        // This causes the Auth screen to return
                        EventBus.Publish(new FinishedLogOutEvent());
                    }
                }
            };

            EventBus.Publish(new ShowPromptEvent($"Switch to {environmentDetails.Name}?", confirmCancelButtonList, promptPlacement));
        }

        /// <summary>
        /// Starts the QR reading system and device camera for authentication.
        /// 
        /// Called via the Unity Editor on the QR Login Button on the AuthenticationScreen prefab
        /// </summary>
        public void StartQrAuthentication()
        {
            DebugUtilities.Log("[AuthenticationScreen] Starting QR Tracking for Authentication");

            QrCodeManager.StartQrTracking("Scan the QR code from the GMS to login");

            QrCodeManager.QrCodeAdded += QrCodeManager_ScanQrCode;
            QrCodeManager.QrCodeUpdated += QrCodeManager_ScanQrCode;
            QrCodeManager.QrCodeSeen += QrCodeManager_QrCodeSeen;
        }

        private async void QrCodeManager_QrCodeSeen(object sender, string e)
        {
            await TryAuthenticationAsync(e);
        }

        /// <summary>
        /// Cancels the QR reading system and device camera for authentication.
        /// 
        /// Called via the Unity Editor
        /// </summary>
        public void CancelQrAuthentication()
        {
            DebugUtilities.Log("[AuthenticationScreen] Stopping QR Tracking for Authentication");

            BringDownEvents();

            QrCodeManager.CancelQrTracking();
        }

        // Authentication only cares about the first seen QR code
        private async void QrCodeManager_ScanQrCode
            (object sender, Microsoft.MixedReality.QR.QRCode e)
        {
            await TryAuthenticationAsync(e.Data);
        }

        private void BringDownEvents()
        {
            QrCodeManager.QrCodeAdded -= QrCodeManager_ScanQrCode;
            QrCodeManager.QrCodeUpdated -= QrCodeManager_ScanQrCode;
            QrCodeManager.QrCodeSeen -= QrCodeManager_QrCodeSeen;
        }

        private async UniTask TryAuthenticationAsync(string qrCode)
        {
            BringDownEvents();

            QrCodeManager.StopQrTracking();

            var environmentalDetails = EnvironmentDetailsScriptableObject.TryCreateEnvironmentViaCode(qrCode);

            // Check to see if the QR code was used to switch environments
            if (environmentalDetails != null)
            {
                PromptEnvironmentSwitch(environmentalDetails);
            }
            // Otherwise, log in as normal
            else
            {
                await AuthenticationManager.AuthenticateWithCode(qrCode);
            }
        }
    }
}