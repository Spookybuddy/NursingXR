using UnityEngine;
using GIGXR.Platform;
using GIGXR.GMS.Clients;
using GIGXR.GMS.Models.Sessions;
using GIGXR.GMS.Models.Sessions.Requests;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Core;
using GIGXR.Platform.ScenarioBuilder.Data;
using GIGXR.Platform.Utilities.SerializableDictionary.Example.Example;
using Newtonsoft.Json.Linq;
using System;
using UnityEditor;
using UnityEngine.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace GIGXR.Platform.ScenarioBuilder.SessionPlanTools
{
    [Serializable]
    [CreateAssetMenu(fileName = "Session Plan Data", menuName = "GIGXR/Scenarios/Scenario & Session Plan Data")]
    public class ScenarioAndSessionDataScriptableObject : ScriptableObject
    {
        public PresetScenario presetScenario;

        [Serializable]
        public enum Options
        {
            DoNothing = 0,
            PublishSessionPlan = 1,
            CreateSessionPlan = 2
        }
        
        [Serializable]
        public enum GMS
        {
            QA,
            PR
        }

        [Serializable]
        public class SessionPlanUploadOptions
        {
            [Header("The name to use when publishing to GMS:")]
            public string sessionPlanName;

            [Header("The action to take when you hit Publish:")]
            public Options action;

            [Header("The target GMS to push the session plan to.")]
            public GMS targetGMS;
        }

        [FormerlySerializedAs("qaSessionPlans")]
        [SerializeField, Header("Key should be the Session Plan GUID")]
        public GenericStringEnumDictionary<SessionPlanUploadOptions> sessionPlans;


#if UNITY_EDITOR
        private GmsApiClient GmsApiClient
        {
            get
            {
                if(_gmsClient == null)
                {
                    var profile = ProfileManager.GetOrCreateSettings();

                    var config = new GmsApiClientConfiguration(
                            productName: Application.productName,
                            productVersion: Application.version,
                            authenticationProfile: profile.authenticationProfile);

                    _gmsClient = new GmsApiClient(new AppEventBus(), config, profile);                    
                }

                return _gmsClient;
            }
        }

        private GmsApiClient _gmsClient;

        [ContextMenu("Export to text file")]
        private void ExportToJsonText()
        {
            Debug.Assert(presetScenario != null, "Please provide a PresetScenario to export.");

            var fileName = presetScenario.presetScenarioName + ".txt";
            var presetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            presetPath = presetPath.Substring(0, presetPath.LastIndexOf('/'));
            var fullPath = Path.Combine(presetPath, fileName);

            var scenario = presetScenario.BuildScenario();

            var json = JsonConvert.SerializeObject(scenario, Formatting.Indented);

            File.WriteAllText(fullPath, json);

            Debug.Log($"Exported to {fullPath}");
        }

        [ContextMenu("Upload to GMS")]
        private async void PostUpdatedSessionPlan()
        {
            var profile = ProfileManager.GetOrCreateSettings();
            
            var scenario = presetScenario.BuildScenario();
            
            // Get app info
            var appVersion = profile.appDetailsProfile.appDetailsScriptableObject.VersionString;
            var testEmail = EditorAuthenticationProfile.GetTestCredentials().Email;
            var testPassword = EditorAuthenticationProfile.GetTestCredentials().Password;

            // Login
            await GmsApiClient.AccountsApi.LoginWithEmailPassAsync(testEmail, testPassword);

            string removedSessionPlanIdString = null;
            Guid createdPlanGuid = Guid.Empty;

            foreach (var sessionPlan in sessionPlans)
            {
                if (sessionPlan.Value.action == Options.PublishSessionPlan)
                {
                    var sessionName = $"{sessionPlan.Value.sessionPlanName}";

                    // Only append the appVersion to QA plans:
                    if (sessionPlan.Value.targetGMS == GMS.QA)
                        sessionName += $" {appVersion}";

                    var plan = new UpdateSessionPlanRequest
                    {
                        SessionName = sessionName,
                        Description = $"Created with app version {appVersion}",
                        HmdJson = JObject.FromObject(scenario),
                        SessionStatus = SessionStatus.Ended,
                        SessionPermission = SessionPermission.OpenToInstitution
                    };

                    var result = await GmsApiClient.SessionsApi.UpdateSessionPlanAsync(new Guid(sessionPlan.Key), plan);

                    if (result == null)
                    {
                        Debug.LogError("Failed to update plan, check your session data and try again. Make sure your test credentials are the same as the ones used to create the plan. Also check out the Session Plan data SO for errors or missing GUIDs.");
                        continue;
                    }
                    else
                    {
                        // Success
                        Debug.LogWarning($"[{sessionPlan.Value.targetGMS}]: Updated Session Plan: {sessionPlan.Key}");
                    }
                }
                else if (sessionPlan.Value.action == Options.CreateSessionPlan)
                {
                    if (!string.IsNullOrEmpty(sessionPlan.Key))
                    {
                        Debug.LogError($"Session plan already has a session ID. " +
                            $"To update an existing plan, select the the appropriate action. " +
                            $"To create a session and get a new ID, remove the existing ID.");
                        continue;
                    }

                    var sessionName = $"{sessionPlan.Value.sessionPlanName}";

                    // Only append the appVersion to QA plans:
                    if (sessionPlan.Value.targetGMS == GMS.QA)
                        sessionName += $" {appVersion}";

                    if (EditorUtility.DisplayDialog("Create session plan?", sessionName, "Confirm", "Cancel"))
                    {
                        var plan = new CreateSessionPlanRequest()
                        {
                            SessionName = sessionName,
                            ClientAppId = Guid.Parse(profile.authenticationProfile.ApplicationId()),
                            Description = $"Created with app version {appVersion}",
                            SessionPermission = SessionPermission.OpenToInstitution,
                            HmdJson = JObject.FromObject(scenario)
                        };

                        var result = await GmsApiClient.SessionsApi.CreateSessionPlanAsync(plan);

                        if (result == null)
                        {
                            Debug.LogError("Failed to create plan, check your session data and try again. Make sure your test credentials can create session plans. Also check out the Session Plan data SO for errors.");
                            continue;
                        }
                        else
                        {
                            removedSessionPlanIdString = sessionPlan.Key;
                            createdPlanGuid = result.SessionId;

                            // Success
                            Debug.LogWarning($"[{sessionPlan.Value.targetGMS}]: Created Session Plan: {createdPlanGuid}");
                        }
                    }
                }
            }

            // Only 1 plan can have been created; only 1 key value pair in the session plans could have an empty key.
            // If a plan was created, update the key value pair appropriately with the new session id.
            if (createdPlanGuid != Guid.Empty)
            {
                var sessionPlan = sessionPlans[removedSessionPlanIdString];
                sessionPlans.Remove(removedSessionPlanIdString);

                sessionPlan.action = Options.DoNothing;
                sessionPlans.Add(createdPlanGuid.ToString(), sessionPlan);

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
            }
        }
#endif
    }
}