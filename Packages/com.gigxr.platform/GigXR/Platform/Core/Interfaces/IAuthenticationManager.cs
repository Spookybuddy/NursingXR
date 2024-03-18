namespace GIGXR.Platform.Interfaces
{
    using Cysharp.Threading.Tasks;
    using GIGXR.GMS.Clients;

    /// <summary>
    /// Interface to help handle authentication with GMS
    /// </summary>
    public interface IAuthenticationManager
    {
        public enum AuthenticationStatus
        {
            Success,
            FailedUnknownReason,
            FailedOnTimeOut,
            Nothing,
            FailedUnauthorized,
            Forbidden
        }

        GmsApiClient GMSApiClient { get; }

        void LogOut();

        void UpdateEnvironment();

        UniTask<AuthenticationStatus> AuthenticateWithCode(string code);

        void AuthenticateWithJsonWebToken(string jsonWebToken);

        void SetJWTHandlerActivity(bool value);
    }
}