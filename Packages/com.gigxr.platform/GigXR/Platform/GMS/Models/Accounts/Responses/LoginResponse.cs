namespace GIGXR.GMS.Models.Accounts.Responses
{
    public class LoginResponse
    {
        public LoginResponse(string jsonWebToken, string callbackPayload = null)
        {
            JsonWebToken = jsonWebToken;
            CallbackPayload = callbackPayload;
        }

        public string JsonWebToken { get; }

        /// <summary>
        /// Additional data that can be sent back from GMS with the log in information.
        /// </summary>
        /// <remarks>Joining a session via QR uses this property to attach the Session ID</remarks>
        public string CallbackPayload { get; }
    }
}