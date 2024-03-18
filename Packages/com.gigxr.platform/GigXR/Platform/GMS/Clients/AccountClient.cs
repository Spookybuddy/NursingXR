namespace GIGXR.GMS.Clients
{
    using System;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Networking;
    using Models.Accounts;
    using Models.Accounts.Requests;
    using Models.Accounts.Responses;
    using Platform.Data;
    using Platform.GMS;
    using System.Globalization;
    using System.Net.Http.Headers;
    using System.Text;
    using UnityEngine;
    using GIGXR.Platform.GMS.Exceptions;
    using GigXR.Platform.GMS.Models.Accounts.Responses;
    using UnityEditor;

    /// <summary>
    /// A client for accessing <c>/accounts</c> endpoints from the GMS API.
    ///
    /// Don't use this class directly, prefer using <see cref="GmsClient"/>.
    /// </summary>
    public sealed partial class AccountApiClient : BaseApiClient
    {
        private readonly GmsWebRequestClient gmsWebRequestClient;
        private readonly GmsApiClientConfiguration configuration;
        private readonly ProfileManager profileManager;

        private ValidJWTHandler validJWTHandler;
        private TimeSpan validDuration;

        public AccountApiClient(AppEventBus eventBus, GmsApiClientConfiguration configuration, GmsWebRequestClient gmsWebRequestClient, ProfileManager profile)
        {
            this.configuration = configuration;
            this.gmsWebRequestClient = gmsWebRequestClient;
            this.profileManager = profile;

            validDuration = TimeSpan.FromSeconds(profileManager.authenticationProfile.TokenValidDurationSeconds);

            // Since Mobile uses the WebView for logging in, the GMS Client view will manage the JWT expiration and refresh
            // Therefore, HMD needs its own way to refresh the JWT and handle expiration
#if !(UNITY_IOS || UNITY_ANDROID)
                validJWTHandler = new ValidJWTHandler(eventBus, this, validDuration);
#endif
        }

        public GmsAccount AuthenticatedAccount { get; private set; }

        public AccountDetailedView CachedAccountDetailedView { get; private set; }

        public bool IsLoggedIn
        {
            get
            {
                return AuthenticatedAccount != null;
            }
        }

        /// <summary>
        /// Attempts authorization to a user's account with their email and password combo and sets up
        /// the AuthenticatedAccount if successful
        /// </summary>
        /// <param name="email">The user's registered email address</param>
        /// <param name="password">The user's password</param>
        public async UniTask LoginWithEmailPassAsync(string email, string password)
        {
            const string path = "accounts/login";
            
            var request = new LoginRequest
            (
                email,
                password,
                configuration.ClientAppId,
                validDuration
            );

            var response = await gmsWebRequestClient.Post<LoginResponse>(path, request.ToJsonString());

            if(response?.JsonWebToken != null)
            {
                var authorizationHeader = new AuthenticationHeaderValue("Bearer", response.JsonWebToken);
                gmsWebRequestClient.Authorization = authorizationHeader;

                await BuildAuthenticatedAccountAsync(response.JsonWebToken);

                validJWTHandler?.Enable();
            }
        }

        /// <summary>
        /// Attempts authorization to a user's account with a QR Code and sets up the AuthenticatedAccount if successful.
        /// </summary>
        /// <param name="qrCode">The QR code as a string</param>
        public async UniTask<LoginResponse> LoginWithQrCodeAsync(string qrCode)
        {
            const string path = "accounts/login/qr";
            var request = new LoginWithQrCodeRequest(qrCode, configuration.ClientAppId, configuration.ClientAppSecret);
            var response = await gmsWebRequestClient.Post<LoginResponse>(path, request.ToJsonString());
            
            // If no response is given, throw an error for unauthorization
            if(response == null || string.IsNullOrEmpty(response.JsonWebToken))
            {
                throw new GmsApiUnauthorizedException("Invalid QR Code");
            }

            var authorizationHeader = new AuthenticationHeaderValue("Bearer", response.JsonWebToken);
            gmsWebRequestClient.Authorization = authorizationHeader;

            await BuildAuthenticatedAccountAsync(response.JsonWebToken);

            validJWTHandler?.Enable();

            return response;
        }

        /// <summary>
        ///  Attempts authorization to a user's account with a JSON Web token and sets up the 
        ///  AuthenticatedAccount if successful.
        /// </summary>
        /// <param name="jsonWebToken"></param>
        /// <returns></returns>
        public async UniTask LoginWithJsonWebTokenAsync(string jsonWebToken)
        {
            // TODO: Verify JWT and if invalid throw a GmsApiUnauthorizedException
            var authorizationHeader = new AuthenticationHeaderValue("Bearer", jsonWebToken);
            gmsWebRequestClient.Authorization = authorizationHeader;

            await BuildAuthenticatedAccountAsync(jsonWebToken);

            validJWTHandler?.Enable();
        }

        /// <summary>
        /// Private method that takes a JWT and builds the user's AuthenticatedAccount details
        /// </summary>
        /// <param name="jsonWebToken">The JWT that is created after they sign in</param>
        private async UniTask BuildAuthenticatedAccountAsync(string jsonWebToken)
        {
            UserData userData = DeserializeJsonWebToken(jsonWebToken);
            var accountId = Guid.Parse(userData.nameid);
            var institutionId = Guid.Parse(userData.institutionId);
            var account = new GmsAccount(accountId, institutionId);
            AuthenticatedAccount = account;

            CachedAccountDetailedView = await GetAccountProfileAsync(accountId);

            validJWTHandler?.SetExpirationForToken(validDuration);
        }

        /// <summary>
        /// Refreshes a user's JWT to prevent them being logged out due to inactivity.
        /// </summary>
        public async UniTask RefreshTokenAsync()
        {
            const string path = "accounts/login/refresh";
            var response = await gmsWebRequestClient.Post<JsonWebTokenWebViewToUnityDto>(path, null);
            var authorizationHeader = new AuthenticationHeaderValue("Bearer", response.jsonWebToken);
            gmsWebRequestClient.Authorization = authorizationHeader;

            validJWTHandler?.SetExpirationForToken(validDuration);
        }

        /// <summary>
        /// Logs a user out of the app and report it to GMS
        /// </summary>
        public async UniTask LogoutAsync()
        {
            const string path = "accounts/logout";
            await gmsWebRequestClient.Post(path, null);

            validJWTHandler?.Disable();
            PlayerPrefs.DeleteKey("tokenExpiration");

            gmsWebRequestClient.Authorization = null;
            CachedAccountDetailedView = null;
            AuthenticatedAccount = null;
        }

        /// <summary>
        /// Gets the institution information based on the provided ID
        /// </summary>
        /// <returns>All the data associated with the institution</returns>
        public async UniTask<InstitutionDetailedView> GetInstitutionInfoById(Guid institutionId)
        {
            string path = "institutions/" + institutionId.ToString();
            var response = await gmsWebRequestClient.Get<InstitutionDetailedView>(path);
            return response;
        }

        /// <summary>
        /// Gets the local user's full account detail
        /// </summary>
        /// <returns>All the data associated with the local user</returns>
        public async UniTask<AccountDetailedView> GetAuthenticatedAccountProfileAsync()
        {
            if (AuthenticatedAccount == null)
            {
                return null;
            }

            var authenticatedProfile = await GetAccountProfileAsync(AuthenticatedAccount.AccountId);

            return authenticatedProfile;
        }

        /// <summary>
        /// Gets the full account details of a particular user
        /// </summary>
        /// <param name="AccountId">The ID of the user of interest</param>
        /// <returns>All the data associated with the given user</returns>
        public async UniTask<AccountDetailedView> GetAccountProfileAsync(Guid AccountId)
        {
            var path = $"accounts/{AccountId}";
            return await gmsWebRequestClient.Get<AccountDetailedView>(path);
        }

        /// <summary>
        /// Converts a JWT to the UserData class so that it can be used in code
        /// </summary>
        /// <param name="jsonWebToken">The JWT generated after logging in</param>
        /// <returns>The UserData which contains information about the user who logged in</returns>
        private UserData DeserializeJsonWebToken(string jsonWebToken)
        {
            int start = jsonWebToken.IndexOf(".", StringComparison.Ordinal) + 1;
            int end = jsonWebToken.LastIndexOf(".", StringComparison.Ordinal);
            string jwtSubstring = jsonWebToken.Substring(start, end - start);

            // ensure the base64 string is off the correct decodable length.
            jwtSubstring = jwtSubstring.Replace(" ", "+");
            int mod4 = jwtSubstring.Length % 4;
            if (mod4 > 0)
            {
                jwtSubstring += new string('=', 4 - mod4);
            }

            // and convert that to json.
            byte[] bytes = Convert.FromBase64String(jwtSubstring);
            string userJson = Encoding.UTF8.GetString(bytes);

            // then extract the JSON to our UserData class
            UserData data = JsonUtility.FromJson<UserData>(userJson);
            return data;
        }

        public void SetJWTHandlerActivity(bool value)
        {
            if(IsLoggedIn)
            {
                if (value)
                {
                    validJWTHandler?.Enable();
                }
                else
                {
                    validJWTHandler?.Disable();
                }
            }
        }
    }

    //
    // TODO: Code below needs to be updated. It is platform 2 legacy.
    //
    public sealed partial class AccountApiClient
    {
        private readonly IAuthenticationManager authenticationManager;

        public AccountApiClient(IAuthenticationManager authenticationManager)
        {
            this.authenticationManager = authenticationManager;
        }

        /// <summary>
        /// Updates GMS with the specified firebase registration token for the
        /// currently authenticated account.
        /// </summary>
        public async UniTask UpdateFirebaseRegistrationTokenAsync(string token)
        {
            try
            {
                var path = $"accounts/{AuthenticatedAccount.AccountId}/firebase-registration-tokens/{token}";
                await gmsWebRequestClient.Post(path, null);
                //TODO Can't do this with UnityWebRequests - response.EnsureSuccessStatusCode();
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
            }
        }

        /// <summary>
        /// Deletes the device's firebase token on GMS for the currently authenticated account,
        /// so that user will not receive notifications on this device.
        /// 
        /// TODO This endpoint doesn't exist in GMS anymore, not sure if we should keep this around
        /// </summary>
        /*public async UniTask DeleteFirebaseRegistrationTokenAsync(string token)
        {
            try
            {
                var path = $"accounts/{AuthenticatedAccount.AccountId}/firebase-registration-tokens/{token}";
                var response = await httpClient.DeleteAsync(path).AsUniTask();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
            }
        }*/
    }
}