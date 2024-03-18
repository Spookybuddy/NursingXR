using Cysharp.Threading.Tasks;
using GIGXR.GMS.Models.Sessions;
using GIGXR.GMS.Models.Sessions.Requests;
using GIGXR.GMS.Models.Sessions.Responses;
using GIGXR.Platform;
using GIGXR.Platform.ScenarioBuilder.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A custom Editor that connects with GMS that allows the Unity Developer access to all of their session plans
/// within the Unity Editor.
/// </summary>
public class SessionPlansEditor : EditorWindow
{
    private static List<SessionPlanListView> sessionPlanList;

    private static SessionDetailedView selectedSessionPlan;

    private static string sessionName;
    private static string description;
    private static string clientVersion;
    private static SessionPermission permission;
    private static HmdJsonProvider scenarioScriptableObject;

    private Vector2 sessionPlanListScrollPos;
    private Vector2 hmdJsonScrollPos;

    private static ProfileManager ProfileSettings
    {
        get
        {
            return ProfileManager.GetOrCreateSettings();
        }
    }

    [MenuItem("GIGXR/Session Plan Manager")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(SessionPlansEditor));

        GetSessionPlans().Forget();

        // Start getting all the session plans ASAP
        window.Show();
    }

    void OnGUI()
    {
        // Have two columns, one for the list of session plans and one for data entry
        EditorGUILayout.BeginHorizontal();

        // Session Plan list column
        EditorGUILayout.BeginVertical();

        sessionPlanListScrollPos = EditorGUILayout.BeginScrollView(sessionPlanListScrollPos);

        if (sessionPlanList != null)
        {
            foreach (var plan in sessionPlanList)
            {
                if (GUILayout.Button(plan.SessionName, GUILayout.ExpandWidth(false)))
                {
                    GetSessionPlan(plan.SessionId).Forget();

                    // Make sure no text boxes have focus
                    GUI.FocusControl(null);
                }
            }
        }
        else
        {
            GUILayout.Label("No Session Plans");
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Refresh"))
        {
            GetSessionPlans().Forget();
        }

        EditorGUILayout.EndVertical();

        // Data entry for session plan data column
        EditorGUILayout.BeginVertical();

        if (selectedSessionPlan != null)
        {
            clientVersion = selectedSessionPlan.ClientAppVersion;
        }
        else
        {
            clientVersion = ProfileManager.GetOrCreateSettings().appDetailsProfile.appDetailsScriptableObject.VersionString;
        }

        EditorGUILayout.LabelField("Session ID", selectedSessionPlan?.SessionId.ToString() ?? Guid.Empty.ToString());
        sessionName = EditorGUILayout.TextField("Session Name", sessionName);
        description = EditorGUILayout.TextField("Description", description);
        permission = (SessionPermission)EditorGUILayout.EnumPopup("Session Permission", permission);
        scenarioScriptableObject = (HmdJsonProvider)EditorGUILayout.ObjectField(scenarioScriptableObject, typeof(HmdJsonProvider), allowSceneObjects: false);

        if (ProfileSettings.authenticationProfile.TargetEnvironmentalDetails.UseVersioning)
        {
            EditorGUILayout.LabelField(clientVersion);
        }

        hmdJsonScrollPos = EditorGUILayout.BeginScrollView(hmdJsonScrollPos);

        EditorGUILayout.HelpBox(selectedSessionPlan?.HmdJson.ToString() ?? "", MessageType.None);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        // Create/Reset/Delete button row
        EditorGUILayout.BeginHorizontal();

        string buttonText;

        if (selectedSessionPlan == null)
        {
            buttonText = "Create";
        }
        else
        {
            buttonText = "Update";
        }

        if (GUILayout.Button(buttonText))
        {
            // Create Session Plan
            if (selectedSessionPlan == null)
            {
                if (EditorUtility.DisplayDialog("Create session plan?", sessionName, "Confirm", "Cancel"))
                {
                    CreateSessionPlanRequest plan;

                    if (ProfileSettings.authenticationProfile.TargetEnvironmentalDetails.UseVersioning)
                    {
                        plan = new CreateSessionPlanRequest()
                        {
                            SessionName = sessionName,
                            ClientAppId = Guid.Parse(ProfileSettings.authenticationProfile.ApplicationId()),
                            Description = description,
                            SessionPermission = permission,
                            ClientAppVersion = clientVersion,
                            HmdJson = scenarioScriptableObject != null ? scenarioScriptableObject.GetDictionary() : new Newtonsoft.Json.Linq.JObject()
                        };
                    }
                    else
                    {
                        plan = new CreateSessionPlanRequest()
                        {
                            SessionName = sessionName,
                            ClientAppId = Guid.Parse(ProfileSettings.authenticationProfile.ApplicationId()),
                            Description = description,
                            SessionPermission = permission,
                            HmdJson = scenarioScriptableObject != null ? scenarioScriptableObject.GetDictionary() : new Newtonsoft.Json.Linq.JObject()
                        };
                    }

                    CreateSessionPlan(plan, () => RefreshUI()).Forget();
                }
            }
            // Update Session Plan
            else
            {
                if (string.IsNullOrEmpty(sessionName))
                {
                    Debug.LogError($"Session Name is required in order to create a session plan.");

                    return;
                }

                if (EditorUtility.DisplayDialog("Update session plan?", sessionName, "Confirm", "Cancel"))
                {
                    var plan = new UpdateSessionPlanRequest()
                    {
                        SessionName = sessionName,
                        Description = description,
                        SessionPermission = permission,
                        HmdJson = scenarioScriptableObject != null ? scenarioScriptableObject.GetDictionary() : selectedSessionPlan.HmdJson,
                        ClassId = selectedSessionPlan.ClassId,
                        InstructorId = selectedSessionPlan.InstructorId,
                        SessionStatus = selectedSessionPlan.SessionStatus
                    };

                    // Send out the new data to GMS
                    UpdateSessionPlan(selectedSessionPlan.SessionId, plan, () => RefreshUI()).Forget();
                }
            }
        }

        // Button to clear all data easily for the user
        if (GUILayout.Button("Clear"))
        {
            ClearData();
        }
        
        // Only remove a session plan if the session plan exists in GMS
        GUI.enabled = selectedSessionPlan != null;

        // Add a button so that the user can remove a session plan
        if (GUILayout.Button("Archive"))
        {
            if (EditorUtility.DisplayDialog("Archive Session Plan?", sessionName, "Confirm", "Cancel"))
            {
                // Send the request to GMS to archive the session
                ArchiveSessionPlan(selectedSessionPlan.SessionId, () => RefreshUI()).Forget();
            }
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void RefreshUI()
    {
        // Make sure no text boxes have focus
        GUI.FocusControl(null);

        // Clear the data for the UI so new data can be entered
        ClearData();

        // Refresh the list of all session plans
        GetSessionPlans().Forget();
    }

    private void ClearData()
    {
        selectedSessionPlan = null;

        sessionName = "";
        description = "";
        clientVersion = "";
        permission = SessionPermission.Private;
        scenarioScriptableObject = null;
    }

    private static async UniTask UpdateSessionPlan(Guid sessionId, UpdateSessionPlanRequest updateRequest, Action successAction)
    {
        var result = await GmsApiEditor.UpdateSessionPlan(sessionId, updateRequest);

        if (result == null)
        {
            Debug.LogError("Failed to update session plan, check your session data and try again. Make sure your test credentials can create session plans. Also check out the Session Plan data SO for errors.");
        }
        else
        {
            // Success
            Debug.Log($"Updated Session Plan: {result.SessionId}");

            successAction?.Invoke();
        }
    }

    private static async UniTask CreateSessionPlan(CreateSessionPlanRequest createRequest, Action successCallback)
    {
        var result = await GmsApiEditor.CreateSessionPlan(createRequest);

        if (result == null)
        {
            Debug.LogError("Failed to create session plan, check your session data and try again. Make sure your test credentials can create session plans. Also check out the Session Plan data SO for errors.");
        }
        else
        {
            // Success
            Debug.Log($"Created Session Plan: {result.SessionId}");

            successCallback?.Invoke();
        }
    }

    private static async UniTask ArchiveSessionPlan(Guid sessionPlanId, Action successCallback)
    {
        await GmsApiEditor.ArchiveSessionPlan(sessionPlanId);

        successCallback?.Invoke();
    }

    private static async UniTask GetSessionPlan(Guid sessionPlanId)
    {
        selectedSessionPlan = await GmsApiEditor.GetSessionPlan(sessionPlanId);

        sessionName = selectedSessionPlan.SessionName;
        description = selectedSessionPlan.Description;
        clientVersion = selectedSessionPlan.ClientAppVersion;
        permission = selectedSessionPlan.SessionPermission;
    }

    private static async UniTask GetSessionPlans()
    {
        sessionPlanList = await GmsApiEditor.GetSessionPlans();
    }
}
