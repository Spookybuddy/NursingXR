namespace GIGXR.GMS.Models.Accounts.Requests
{
    using System;

    public class LoginWithQrCodeRequest
    {
        public LoginWithQrCodeRequest(string qrCode, Guid clientAppId, string clientAppSecret)
        {
            QrCode = qrCode;
            ClientAppId = clientAppId;
            ClientAppSecret = clientAppSecret;
        }

        public string QrCode { get; set; }
        public Guid ClientAppId { get; set; }
        public string ClientAppSecret { get; set; } = null!;
    }
}