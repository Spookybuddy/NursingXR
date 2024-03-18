namespace GIGXR.Platform.Core
{
    using Newtonsoft.Json;
    using System;
    using UnityEngine;

    /// <summary>
    /// Holds environmental details related to connecting to the GMS API endpoints.
    /// </summary>
    [CreateAssetMenu
        (fileName = "Environment Detail",
         menuName = "GIGXR/ScriptableObjects/New Environment Detail")]
    public class EnvironmentDetailsScriptableObject : ScriptableObject
    {
        public string Name;
        public string Api = "/api/v1/";
        public string URL;
        public string AppID;
        public string ClientSecret;
        [Obsolete("Use encoding the entire environment details into a QR code instead.")]
        public string SwitchToThisEnvironmentCode;
        [Tooltip("Optional. If set, will include this version number in requests sent to GMS. Otherwise, " +
            "no version will be sent and GMS will default to v1.0 payloads. Use ('1.0','1.1')")]
        public string GmsVersion;
        // If not true, will assume the environment is Production and not include things like the Debugger views
        public bool IsQAEnvironment;

        public bool UseVersioning
        {
            get
            {
                return !string.IsNullOrEmpty(GmsVersion) && GmsVersion != "1.0";
            }
        }

        public void FromDto(EnvironmentDetailsDTO dto)
        {
            Name = dto.Name;
            Api = dto.Api;
            URL = dto.URL;
            AppID = dto.AppID;
            ClientSecret = dto.ClientSecret;
            GmsVersion = dto.GmsVersion;
            IsQAEnvironment = dto.IsQAEnvironment;
        }

        public EnvironmentDetailsDTO ToDto()
        {
            return new EnvironmentDetailsDTO()
            {
                Name = Name,
                Api = Api,
                URL = URL,
                AppID = AppID,
                ClientSecret = ClientSecret,
                GmsVersion = GmsVersion,
                IsQAEnvironment = IsQAEnvironment
            };
        }

        public static EnvironmentDetailsScriptableObject TryCreateEnvironmentViaCode(string code)
        {
            EnvironmentDetailsScriptableObject container = null;

            try
            {
                var dto = JsonConvert.DeserializeObject<EnvironmentDetailsDTO>(code);

                if (dto != null)
                {
                    container = ScriptableObject.CreateInstance<EnvironmentDetailsScriptableObject>();
                    container.FromDto(dto);
                }

                return container;
            }
            catch
            {
                return null;
            }
        }

        [ContextMenu("Print Details")]
        public void PrintAsJson()
        {
            Debug.Log($"{JsonConvert.SerializeObject(ToDto(), Formatting.Indented)}");
        }
    }

    public class EnvironmentDetailsDTO
    {
        public string Name;
        public string Api;
        public string URL;
        public string AppID;
        public string ClientSecret;
        public string? GmsVersion;
        public bool IsQAEnvironment;
    }
}