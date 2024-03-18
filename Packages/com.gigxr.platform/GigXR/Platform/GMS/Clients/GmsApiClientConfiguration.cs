namespace GIGXR.GMS.Clients
{
    using GIGXR.Platform.Core.Settings;
    using System;
    using System.Net.Http.Headers;

    public class GmsApiClientConfiguration
    {
        private AuthenticationProfile authenticationProfile;

        public GmsApiClientConfiguration(
            string productName,
            string productVersion,
            AuthenticationProfile authenticationProfile)
        {
            this.authenticationProfile = authenticationProfile;

            UserAgent = new ProductInfoHeaderValue(SanitizeProductInfoNameHeaderValue(productName), productVersion);
        }

        public ProductInfoHeaderValue UserAgent { get; }

        public Uri BaseAddress 
        { 
            get
            {
                return new Uri(NormalizeUrlTrailingSlash(authenticationProfile.ApiUrl())); 
            }
        }

        public Guid ClientAppId 
        { 
            get 
            {
                return Guid.Parse(authenticationProfile.ApplicationId());
            }
        }

        public string ClientAppSecret
        { 
            get 
            {
                return authenticationProfile.ClientSecret();
            }
        }

        public string GmsVersion 
        { 
            get
            {
                return authenticationProfile.GmsVersion();
            }
        }

        /// <summary>
        /// Append a slash to the URL if not already present.
        /// 
        /// https://stackoverflow.com/questions/23438416/why-is-httpclient-baseaddress-not-working
        /// https://dev.to/jonassamuelsson/constructing-uris-in-dotnet-is-harder-than-it-should-35ep
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string NormalizeUrlTrailingSlash(string url)
        {
            url = url.Trim();

            if (url[url.Length - 1] != '/')
            {
                url += '/';
            }

            return url;
        }

        private string SanitizeProductInfoNameHeaderValue(string productName)
        {
            // Spaces and periods are invalid characters in a User-Agent header.
            return productName.Replace(" ", "").Replace(".", "");
        }
    }
}