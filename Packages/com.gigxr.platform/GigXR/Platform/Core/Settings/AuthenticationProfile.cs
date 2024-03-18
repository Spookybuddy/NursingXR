namespace GIGXR.Platform.Core.Settings
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using UnityEngine;

    [Serializable]
    public class AuthenticationProfile
    {
        /// <summary>
        /// How long the profile's JWT Token will remain valid, expressed in seconds.
        /// </summary>
        public int TokenValidDurationSeconds = 3600;

        /// <summary>
        /// Returns the URL to open the external Terms and Conditions URL.
        /// </summary>
        public string TermsAndConditionsURL = "https://www.gigxr.com/privacy";

        /// <summary>
        /// Returns the URL to open the external Credits URL.
        /// </summary>
        public string CreditsURL = "https://www.gigxr.com/credits";

        public EnvironmentDetailsScriptableObject TargetEnvironmentalDetails;

        public EnvironmentDetailsScriptableObject[] EnvironmentalDetailOptions;

        /// <summary>
        /// Returns the GMS authentication URL with API version.
        /// </summary>
        public string ApiUrl()
        {
            return string.Concat(TargetEnvironmentalDetails.URL, TargetEnvironmentalDetails.Api);
        }

        /// <summary>
        /// Returns the GMS authentication URL.
        /// </summary>
        public string GmsUrl()
        {
            return TargetEnvironmentalDetails.URL;
        }

        /// <summary>
        /// Returns the ApplicationID.
        /// </summary>
        public string ApplicationId()
        {
            return TargetEnvironmentalDetails.AppID;
        }

        /// <summary>
        /// Returns the ClientSecret.
        /// </summary>
        public string ClientSecret()
        {
            return TargetEnvironmentalDetails.ClientSecret;
        }

        public string GmsVersion()
        {
            // Return null explicitly if no value is given, on the back end, v1.0 will be used
            if (string.IsNullOrEmpty(TargetEnvironmentalDetails.GmsVersion))
                return null;

            return TargetEnvironmentalDetails.GmsVersion;
        }

        /// <summary>
        /// Used by Pigeon.
        /// </summary>
        /// <param name="target"></param>
        public void SetTargetGMS(EnvironmentDetailsScriptableObject environmentDetails)
        {
            TargetEnvironmentalDetails = environmentDetails;
        }

        public EnvironmentDetailsScriptableObject GetEnvironmentByName(string environmentName)
        {
            foreach (var currentEnvironment in EnvironmentalDetailOptions)
            {
                if (currentEnvironment.Name == environmentName)
                {
                    return currentEnvironment;
                }
            }

            return null;
        }

        public EnvironmentDetailsScriptableObject GetEnvironmentByQrCode(string code)
        {
            foreach (var currentEnvironment in EnvironmentalDetailOptions)
            {
                if (currentEnvironment.SwitchToThisEnvironmentCode == code)
                {
                    return currentEnvironment;
                }
            }

            return null;
        }

        public bool TrySetNewEnvironmentViaCode(string code)
        {
            var environmentDetails = GetEnvironmentByQrCode(code);

            if (environmentDetails != null)
            {
                SetTargetGMS(environmentDetails);

                return true;
            }

            return false;
        }
    }
}