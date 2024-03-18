namespace GIGXR.GMS.Models.Accounts.Requests
{
    using System;

    public class LoginRequest
    {
        public LoginRequest(string email, string password, Guid clientAppId, TimeSpan? validDuration)
        {
            Email = email;
            Password = password;
            ClientAppId = clientAppId;
            ValidDuration = validDuration;
        }

        public string Email { get; }
        public string Password { get; }
        public Guid ClientAppId { get; }
        public TimeSpan? ValidDuration { get; set; }
    }
}