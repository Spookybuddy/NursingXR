namespace GIGXR.Platform.Managers
{
    using UnityEngine;
    using AppEvents.Events.Authentication;
    using GIGXR.GMS.Clients;
    using GIGXR.Platform.GMS.Exceptions;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Networking;
    using System;
    using System.Net.Http;
    using GIGXR.Platform.AppEvents;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using GIGXR.GMS.Models.Accounts.Responses;
    using GIGXR.Platform.AppEvents.Events.Session;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core;
    using GIGXR.Platform.Core.FeatureManagement;

    /// <summary>
    /// Responsible for handling the app's connections with Authentication related endpoints.
    /// </summary>
    public class AuthenticationManager : IAuthenticationManager, IDisposable
    {
        #region Dependencies

        protected AppEventBus EventBus { get; }

        public INetworkManager NetworkManager { get; }

        public IFeatureManager FeatureManager { get; }

        public GmsApiClient GMSApiClient { get; }

        public ProfileManager ProfileManager { get; }

        #endregion

        public AuthenticationManager(INetworkManager networkManager, AppEventBus appEvent, GmsApiClient apiClient,
            ProfileManager profileManager, IFeatureManager featureManager)
        {
            NetworkManager = networkManager;
            GMSApiClient = apiClient;
            EventBus = appEvent;
            ProfileManager = profileManager;
            FeatureManager = featureManager;

            EventBus.Subscribe<BeginAuthenticatingEvent>(OnBeginAuthenticatingEvent);
            EventBus.Subscribe<StartLogOutEvent>(OnStartLogOutEvent);

            // Makes sure that Prod environments don't start with the Debug displays
            UpdateEnvironment();
        }

        #region IDisposableImplementation

        public void Dispose()
        {
            EventBus.Unsubscribe<BeginAuthenticatingEvent>(OnBeginAuthenticatingEvent);
            EventBus.Unsubscribe<StartLogOutEvent>(OnStartLogOutEvent);
        }

        #endregion

        #region Event Bus

        private void OnBeginAuthenticatingEvent(BeginAuthenticatingEvent eventArgs)
        {
            LoginUsingUsernameAndPassword(eventArgs.email, eventArgs.password);
        }

        private void OnStartLogOutEvent(StartLogOutEvent @event)
        {
            if (GMSApiClient.AccountsApi.AuthenticatedAccount != null)
            {
                LogOut();
            }
            else
            {
                // Make sure the user is returned to the Authentication Screen since they are not logged in
                EventBus.Publish(new FinishedLogOutEvent());
            }
        }

        #endregion

        // --- Public Properties:

        /// <summary>
        /// Attempts authentication with a username and password.
        /// </summary>
        private async void LoginUsingUsernameAndPassword(string username, string password)
        {
            try
            {
                await GMSApiClient.AccountsApi.LoginWithEmailPassAsync(username, password);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                switch (e)
                {
                    case GmsApiUnauthorizedException:
                        // GMS has marked the user as unauthorized, pass to the rest of the app
                        EventBus.Publish
                        (
                            new AuthenticatedUserEvent
                            (
                                IAuthenticationManager.AuthenticationStatus.FailedUnauthorized,
                                null,
                                AuthenticationMethod.MANUAL
                            )
                        );
                        break;

                    case HttpRequestException:
                        // If the user is not connected to the an active internet connect, then they will not be able to
                        // send out message, send a TimedOut status so the user can check their internet connect
                        EventBus.Publish
                        (
                            new AuthenticatedUserEvent
                                (IAuthenticationManager.AuthenticationStatus.FailedOnTimeOut, null, AuthenticationMethod.MANUAL)
                        );
                        break;

                    default:
                        EventBus.Publish
                        (
                            new AuthenticatedUserEvent
                            (
                                IAuthenticationManager.AuthenticationStatus.FailedUnknownReason,
                                null,
                                AuthenticationMethod.MANUAL
                            )
                        );
                        break;
                }

                // TODO When to publish IAuthenticationManager.AuthenticationStatus.Forbidden?

                return;
            }

            IAuthenticationManager.AuthenticationStatus userNamePasswordLoginStatus =
                GMSApiClient.AccountsApi.AuthenticatedAccount != null
                    ? IAuthenticationManager.AuthenticationStatus.Success
                    : IAuthenticationManager.AuthenticationStatus.FailedOnTimeOut;

            await AuthorizeNetworkCredentials(userNamePasswordLoginStatus);

            DebugUtilities.Log($"Authentication Status: {userNamePasswordLoginStatus}");
            EventBus.Publish(new AuthenticatedUserEvent(userNamePasswordLoginStatus, null, AuthenticationMethod.MANUAL));
        }

        /// <summary>
        /// Logs the user out of the app, and closes/leaves any session they may be in.
        /// </summary>
        public void LogOut()
        {
            // Since the user will be logged out when disconnected, do not await this command, otherwise it will block the authentication UI
            // from appearing when the user is logged out from a disconnection and there is no network connection
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            GMSApiClient.AccountsApi.LogoutAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            EventBus.Publish(new FinishedLogOutEvent());
        }

        public void UpdateEnvironment()
        {
            // We only allow the Debug details in QA environments
            if (ProfileManager.authenticationProfile.TargetEnvironmentalDetails.IsQAEnvironment)
            {
                FeatureManager.AddRuntimeFeature(Core.Settings.FeatureFlags.DisplayDebugObjects);
            }
            else
            {
                FeatureManager.RemoveRuntimeFeature(Core.Settings.FeatureFlags.DisplayDebugObjects);
            }
        }

        /// <summary>
        /// Attempts to log in using a single use 8 digit code from a QR code.
        /// </summary>
        /// <param name="code"></param>
        public async UniTask<IAuthenticationManager.AuthenticationStatus> AuthenticateWithCode(string code)
        {
            // for dev purposes, if the user scans a specific QR code known only to us, switch them to the QA instance of the GMS.
            if (ProfileManager.authenticationProfile.TrySetNewEnvironmentViaCode(code))
            {
                UpdateEnvironment();

                return IAuthenticationManager.AuthenticationStatus.Nothing;
            }

            Debug.Log("Attempting authentication via decoded QR code.");

            LoginResponse loginResponse = null;

            try
            {
                loginResponse = await GMSApiClient.AccountsApi.LoginWithQrCodeAsync(code);
            }
            catch (Exception e)
            {
                if (e is GmsApiUnauthorizedException unauthorized)
                {
                    EventBus.Publish
                    (
                        new AuthenticatedUserEvent
                        (
                            IAuthenticationManager.AuthenticationStatus.FailedUnauthorized,
                            e.Message,
                            AuthenticationMethod.QRCODE
                        )
                    );
                    return IAuthenticationManager.AuthenticationStatus.FailedUnauthorized;
                }
                else
                {
                    EventBus.Publish
                    (
                        new AuthenticatedUserEvent
                            (IAuthenticationManager.AuthenticationStatus.FailedUnknownReason, null, AuthenticationMethod.QRCODE)
                    );

                    return IAuthenticationManager.AuthenticationStatus.FailedUnknownReason;
                }
            }

            IAuthenticationManager.AuthenticationStatus qrLoginStatus;

            if (GMSApiClient.AccountsApi.AuthenticatedAccount != null)
            {
                qrLoginStatus = IAuthenticationManager.AuthenticationStatus.Success;

                // Pass any QR encoded session data to other classes that may use it
                if (!string.IsNullOrEmpty(loginResponse?.CallbackPayload))
                {
                    // Right now we only support getting additional data from the QR login that adds 'JOIN SESSION <UUID>'
                    var sessionId = loginResponse?.CallbackPayload.Substring(13);
                    EventBus.Publish(new JoinSessionFromQrEvent(Guid.Parse(sessionId)));
                }
            }
            else
            {
                EventBus.Publish
                (
                    new AuthenticatedUserEvent
                        (IAuthenticationManager.AuthenticationStatus.FailedUnknownReason, null, AuthenticationMethod.QRCODE)
                );
                qrLoginStatus = IAuthenticationManager.AuthenticationStatus.FailedUnknownReason;
            }

            Debug.Log($"Authentication Status (QR): {qrLoginStatus}");

            await AuthorizeNetworkCredentials(qrLoginStatus);

            EventBus.Publish(new AuthenticatedUserEvent(qrLoginStatus, null, AuthenticationMethod.QRCODE));

            IAuthenticationManager.AuthenticationStatus authStatus = GMSApiClient.AccountsApi.AuthenticatedAccount != null
                ? IAuthenticationManager.AuthenticationStatus.Success
                : IAuthenticationManager.AuthenticationStatus.FailedUnknownReason;

            return authStatus;
        }

        private async UniTask AuthorizeNetworkCredentials(IAuthenticationManager.AuthenticationStatus status)
        {
            // Set up the user credentials for Photon
            if (status == IAuthenticationManager.AuthenticationStatus.Success)
            {
                var accountInfo = await GMSApiClient.AccountsApi.GetAuthenticatedAccountProfileAsync();

                NetworkManager.SetUser
                    (GMSApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString(), accountInfo.FirstName ?? "User");

                await NetworkManager.ConnectAsync();
            }
        }

        /// <summary>
        ///     Authenticate the user with a Json Web Token. Called when a
        ///     token is received from the GMS, after a mobile user starts
        ///     the app after electing to stay signed in, or when the user
        ///     restarts the app within 5 minutes of their initial sign-in.
        /// </summary>
        public async void AuthenticateWithJsonWebToken(string jsonWebToken)
        {
            try
            {
                await GMSApiClient.AccountsApi.LoginWithJsonWebTokenAsync(jsonWebToken);
            }
            catch (GmsApiUnauthorizedException e)
            {
                Debug.LogException(e);

                // GMS has marked the user as unauthorized, pass to the rest of the app
                EventBus.Publish
                (
                    new AuthenticatedUserEvent
                        (IAuthenticationManager.AuthenticationStatus.FailedUnauthorized, null, AuthenticationMethod.MANUAL)
                );
                return;
            }

            IAuthenticationManager.AuthenticationStatus status = GMSApiClient.AccountsApi.AuthenticatedAccount == null
                ? IAuthenticationManager.AuthenticationStatus.FailedUnknownReason
                : IAuthenticationManager.AuthenticationStatus.Success;

            await AuthorizeNetworkCredentials(status);

            EventBus.Publish(new AuthenticatedUserEvent(status, null, AuthenticationMethod.MANUAL));
        }

        public void SetJWTHandlerActivity(bool value)
        {
            GMSApiClient.AccountsApi.SetJWTHandlerActivity(value);
        }
    }
}