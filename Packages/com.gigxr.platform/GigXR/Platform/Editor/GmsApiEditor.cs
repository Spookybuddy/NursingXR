using Cysharp.Threading.Tasks;
using GIGXR.GMS.Clients;
using GIGXR.GMS.Models.Sessions;
using GIGXR.GMS.Models.Sessions.Requests;
using GIGXR.GMS.Models.Sessions.Responses;
using GIGXR.Platform.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GIGXR.Platform
{
    /// <summary>
    /// Class to help set up some connections to the GMS Endpoints via the Unity Editor.
    /// </summary>
    public class GmsApiEditor
    {
        private static GmsApiClient GmsApiClient
        {
            get
            {
                if(_apiClient == null)
                {
                    var config = new GmsApiClientConfiguration(
                        productName: Application.productName,
                        productVersion: Application.version,
                        authenticationProfile: ProfileManager.authenticationProfile);

                    _apiClient = new GmsApiClient(null, config, ProfileManager);
                }
                
                return _apiClient;
            }
        }

        private static GmsApiClient _apiClient;

        private static ProfileManager ProfileManager
        {
            get
            {
                return ProfileManager.GetOrCreateSettings();
            }
        }

        [MenuItem("GIGXR/GMS/Update App Versions")]
        public static void UpdateGmsVersion()
        {
            PostUpdatedAppVersionToGms();
        }

        [MenuItem("GIGXR/GMS/List App Versions")]
        public static void ListGmsVersions()
        {
            ListAppVersionFromGms();
        }

        public static async void PostUpdatedAppVersionToGms()
        {
            await LogInIfNeeded();

            if (EditorUtility.DisplayDialog("Update GMS?", 
                                            $"Are you sure you want to send the version {ProfileManager.appDetailsProfile.appDetailsScriptableObject.VersionString} to GMS?", 
                                            "Ok", 
                                            "Cancel"))
            {
                var updateVersion = await GmsApiClient.ClientApps.CreateClientAppVersionRequest(ProfileManager.appDetailsProfile.appDetailsScriptableObject.VersionString);

                if (updateVersion != null)
                {
                    Debug.Log($"Updated {updateVersion.ClientAppId} with {updateVersion.Version}");
                }
                else
                {
                    Debug.LogWarning($"Something went wrong when trying to create app version {ProfileManager.appDetailsProfile.appDetailsScriptableObject.VersionString}.");
                }
            }            
        }

        public static async void ListAppVersionFromGms()
        {
            await LogInIfNeeded();

            var versions = await GmsApiClient.ClientApps.GetAppVersions();

            foreach (var v in versions)
            {
                Debug.Log($"Version: {v}");
            }
        }

        public static async UniTask<SessionDetailedView> UpdateSessionPlan(Guid sessionId, UpdateSessionPlanRequest sessionPlanRequest)
        {
            await LogInIfNeeded();

            return await GmsApiClient.SessionsApi.UpdateSessionPlanAsync(sessionId, sessionPlanRequest);
        }

        public static async UniTask<SessionDetailedView> CreateSessionPlan(CreateSessionPlanRequest sessionPlanRequest)
        {
            await LogInIfNeeded();

            return await GmsApiClient.SessionsApi.CreateSessionPlanAsync(sessionPlanRequest);
        }

        public static async UniTask ArchiveSessionPlan(Guid sessionPlanId)
        {
            var patchSession = new PatchSessionRequest()
            {
                SessionId = sessionPlanId,
                SessionStatus = SessionStatus.Archived
            };

            await GmsApiClient.SessionsApi.PatchSessionPlansAsync(new List<PatchSessionRequest>() { patchSession });
        }

        public static async UniTask<SessionDetailedView> GetSessionPlan(Guid sessionPlanId)
        {
            await LogInIfNeeded();

            return await GmsApiClient.SessionsApi.GetSessionPlanAsync(sessionPlanId);
        }

        public static async UniTask<List<SessionPlanListView>> GetSessionPlans()
        {
            await LogInIfNeeded();

            return await GmsApiClient.SessionsApi.GetSessionPlansAsync();
        }

        private static async UniTask LogInIfNeeded()
        {
            var testEmail = EditorAuthenticationProfile.GetTestCredentials().Email;
            var testPassword = EditorAuthenticationProfile.GetTestCredentials().Password;

            await GmsApiClient.AccountsApi.LoginWithEmailPassAsync(testEmail, testPassword);
        }
    }
}
