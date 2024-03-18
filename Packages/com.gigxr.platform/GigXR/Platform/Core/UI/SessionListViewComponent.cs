using Cysharp.Threading.Tasks;
using GIGXR.GMS.Clients;
using GIGXR.GMS.Models.Sessions.Responses;
using GIGXR.Platform.Core.DependencyValidator;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GIGXR.Platform.UI
{
    /// <summary>
    /// Component to hold a SessionListView data.
    /// </summary>
    public class SessionListViewComponent : MonoBehaviour
    {
        [SerializeField, RequireDependency]
        private Interactable interactable;

        [SerializeField, RequireDependency]
        private Image background;

        // TODO Decide if we want to move this type of info outside of this class
        [SerializeField]
        private Color compatibleBackgroundColor;

        // TODO Decide if we want to move this type of info outside of this class
        [SerializeField]
        private Color incompatibleBackgroundColor;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI sessionNameText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI sessionPrimaryLineText;

        // See Figma Documentation for what these lines represent: https://www.figma.com/file/iUnrnJHda9LzqVu0K5UGuU/HS-Versioning-and-restrictions?node-id=86%3A114
        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineOneLabelText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineOneStatusText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineTwoLabelText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineTwoStatusText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineThreeLabelText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineThreeStatusText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineFourLabelText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineFourStatusText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineFiveLabelText;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI lineFiveStatusText;

        [SerializeField, RequireDependency]
        private GameObject incompatibleLabel;

        private SessionListView session;

        private SessionPlanListView sessionPlan;

        private const string timeFormat = @"yyyy/MM/dd HH:mm";

        public async void SetActiveSessionListView(GmsApiClient apiClient, SessionListView sessionListViewComponent)
        {
            session = sessionListViewComponent;
            sessionPlan = null;

            sessionNameText.text = sessionListViewComponent.SessionName;

            var createdByName = await GetSessionUserName(apiClient, sessionListViewComponent);

            sessionPrimaryLineText.text = $"Created by: {createdByName}";

            lineOneLabelText.gameObject.SetActive(true);
            lineOneStatusText.gameObject.SetActive(true);
            lineTwoLabelText.gameObject.SetActive(true);
            lineTwoStatusText.gameObject.SetActive(true);
            lineThreeLabelText.gameObject.SetActive(true);
            lineThreeStatusText.gameObject.SetActive(true);

            lineOneLabelText.text = "Status:";

            lineOneStatusText.text = GetStatusText(sessionListViewComponent);

            lineTwoLabelText.text = "Created by:";

            lineTwoStatusText.text = createdByName;

            lineThreeLabelText.text = "Version:";

            if(string.IsNullOrEmpty(sessionListViewComponent.ClientAppVersion))
            {
                lineThreeStatusText.text = "Alpha";
            }
            else
            {
                lineThreeStatusText.text = sessionListViewComponent.ClientAppVersion;
            }

            // TODO Module
            lineFourLabelText.gameObject.SetActive(false);
            lineFourStatusText.gameObject.SetActive(false);

            // Not used
            lineFiveLabelText.gameObject.SetActive(false);
            lineFiveStatusText.gameObject.SetActive(false);

            interactable.RefreshSetup();
        }

        private async UniTask<string> GetSessionUserName(GmsApiClient apiClient, SessionListView sessionListViewComponent)
        {
            if (apiClient.AccountsApi.AuthenticatedAccount.AccountId == sessionListViewComponent.CreatedById)
            {
                return "You";
            }
            else
            {
                return await GetCreatorName(apiClient, sessionListViewComponent.CreatedById);
            }
        }

        public async void SetSavedSessionListView(GmsApiClient apiClient, SessionListView savedSessionListView)
        {
            session = savedSessionListView;
            sessionPlan = null;

            sessionNameText.text = savedSessionListView.SessionName;

            string statusText = GetStatusText(savedSessionListView);

            sessionPrimaryLineText.text = statusText;

            lineOneLabelText.gameObject.SetActive(true);
            lineOneStatusText.gameObject.SetActive(true);
            lineTwoLabelText.gameObject.SetActive(true);
            lineTwoStatusText.gameObject.SetActive(true);
            lineThreeLabelText.gameObject.SetActive(true);
            lineThreeStatusText.gameObject.SetActive(true);
            lineFourLabelText.gameObject.SetActive(true);
            lineFourStatusText.gameObject.SetActive(true);

            lineOneLabelText.text = "Status:";
            lineOneStatusText.text = statusText;

            lineTwoLabelText.text = "Created at:";
            lineTwoStatusText.text = session.CreatedOn.ToString(timeFormat);

            lineThreeLabelText.text = "Created by:";

            if (apiClient.AccountsApi.AuthenticatedAccount.AccountId == savedSessionListView.CreatedById)
            {
                lineThreeStatusText.text = "You";
            }
            else
            {
                lineThreeStatusText.text = await GetCreatorName(apiClient, savedSessionListView.CreatedById);
            }

            lineFourLabelText.text = "Version:";

            if (string.IsNullOrEmpty(savedSessionListView.ClientAppVersion))
            {
                lineFourStatusText.text = "Alpha";
            }
            else
            {
                lineFourStatusText.text = savedSessionListView.ClientAppVersion;
            }

            // TODO Module
            lineFiveLabelText.gameObject.SetActive(false);
            lineFiveStatusText.gameObject.SetActive(false);

            interactable.RefreshSetup();
        }

        private async UniTask<string> GetCreatorName(GmsApiClient apiClient, Guid creatorId)
        {
            var creatorInfo = await apiClient.AccountsApi.GetAccountProfileAsync(creatorId);

            if (creatorInfo != null)
                return $"{creatorInfo.FirstName} {creatorInfo.LastName}";
            else
            {
                Debug.LogWarning($"[SessionListViewComponent] Could not retrieve account details for User ID {creatorId}.");

                return "";
            }
        }

        public async void SetSessionPlanListView(GmsApiClient apiClient, SessionPlanListView sessionPlanListView)
        {
            sessionPlan = sessionPlanListView;
            session = null;

            sessionNameText.text = sessionPlanListView.SessionName;

            var creatorName = "";
            var createdBy = "Created by:";

            // If a session plan is branded, then the AccountAPI potentially won't be able to grab the account details
            // if they are part of a different institution, so instead just put a text specified by product
            if(sessionPlanListView.GigXrBranded)
            {
                sessionPrimaryLineText.text = "Shared by GigXR";

                lineOneLabelText.text = "Shared by GigXR";
                lineOneStatusText.text = "";
            }
            else
            {
                if (apiClient.AccountsApi.AuthenticatedAccount.AccountId == sessionPlanListView.CreatedById)
                {
                    creatorName = "You";
                }
                else
                {
                    creatorName = await GetCreatorName(apiClient, sessionPlanListView.CreatedById);
                }

                sessionPrimaryLineText.text = $"{createdBy} {creatorName}";

                lineOneLabelText.text = createdBy;
                lineOneStatusText.text = creatorName;
            }

            lineTwoLabelText.gameObject.SetActive(true);
            lineTwoStatusText.gameObject.SetActive(true);

            lineTwoLabelText.text = "Version:";

            if (string.IsNullOrEmpty(sessionPlanListView.ClientAppVersion))
            {
                lineTwoStatusText.text = "Alpha";
            }
            else
            {
                lineTwoStatusText.text = sessionPlanListView.ClientAppVersion;
            }

            // TODO Module, 2 are unused
            lineThreeLabelText.gameObject.SetActive(false);
            lineThreeStatusText.gameObject.SetActive(false);
            lineFourLabelText.gameObject.SetActive(false);
            lineFourStatusText.gameObject.SetActive(false);
            lineFiveLabelText.gameObject.SetActive(false);
            lineFiveStatusText.gameObject.SetActive(false);

            interactable.RefreshSetup();
        }

        private string GetStatusText(SessionListView sessionListViewComponent)
        {
            if (sessionListViewComponent.SessionStatus == GIGXR.GMS.Models.Sessions.SessionStatus.InProgress)
            {
                return GetLessonDateString(sessionListViewComponent, "Started");
            }
            else if (sessionListViewComponent.SessionStatus == GIGXR.GMS.Models.Sessions.SessionStatus.New)
            {
                return GetLessonDateString(sessionListViewComponent, "Scheduled");
            }
            // This shouldn't happen often, GMS should not send ended sessions except for saved sessions
            else if (sessionListViewComponent.SessionStatus == GIGXR.GMS.Models.Sessions.SessionStatus.Ended)
            {
                // We're assuming the last time the session was updated was when it was ended, this may be false if they edit the session
                // in some other way but don't restart the session, but that's probably ok for now
                var endedDifference = DateTime.UtcNow - sessionListViewComponent.ModifiedOn;

                return BuildTimeDifferenceString("Ended", endedDifference, "ago");
            }
            else if (sessionListViewComponent.LessonDate.HasValue)
            {
                return GetLessonDateString(sessionListViewComponent, "Scheduled");
            }
            // Fallback, shouldn't reach this
            else
            {
                return $"Created at {sessionListViewComponent.CreatedOn.ToString(timeFormat)}";
            }
        }

        private string GetLessonDateString(SessionListView sessionListViewComponent, string endText)
        {
            TimeSpan timeDifference;

            if(sessionListViewComponent.LessonDate.HasValue)
            {
                if (DateTime.UtcNow < sessionListViewComponent.LessonDate.Value)
                {
                    timeDifference = sessionListViewComponent.LessonDate.Value - DateTime.UtcNow;

                    return BuildTimeDifferenceString("Starts in", timeDifference);
                }
                else
                {
                    timeDifference = DateTime.UtcNow - sessionListViewComponent.LessonDate.Value;

                    return BuildTimeDifferenceString(endText, timeDifference, "ago");
                }
            }
            // Fallback, should not reach this
            else
            {
                return $"Created at {sessionListViewComponent.CreatedOn.ToString(timeFormat)}";
            }    
        }

        private string BuildTimeDifferenceString(string pretext, TimeSpan endedDifference, string suffixText = "")
        {
            if (endedDifference.TotalSeconds < 60)
            {
                int totalSeconds = Mathf.RoundToInt((float)endedDifference.TotalSeconds);

                return $"{pretext} {totalSeconds} {TryPlural("second", totalSeconds)} {suffixText}";
            }
            else if (endedDifference.TotalMinutes < 60)
            {
                int totalMinutes = Mathf.RoundToInt((float)endedDifference.TotalMinutes);

                return $"{pretext} {totalMinutes} {TryPlural("minute", totalMinutes)} {suffixText}";
            }
            else if (endedDifference.TotalHours < 24)
            {
                int totalHours = Mathf.RoundToInt((float)endedDifference.TotalHours);

                return $"{pretext} {totalHours} {TryPlural("hour", totalHours)} {suffixText}";
            }
            else if (endedDifference.TotalDays < 365)
            {
                int totalDays = Mathf.RoundToInt((float)endedDifference.TotalDays);

                return $"{pretext} {totalDays} {TryPlural("day", totalDays)} {suffixText}";
            }
            else
            {
                int totalYears = Mathf.RoundToInt((float)endedDifference.TotalDays) / 365;

                return $"{pretext} {totalYears} {TryPlural("year", totalYears)} {suffixText}";
            }
        }

        /// <summary>
        /// Adds an s if the count is above 1.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private string TryPlural(string value, int count)
        {
            if (count > 1)
                return value + "s";
            else
                return value;
        }

        public Guid GetSessionId()
        {
            if(session != null)
                return session.SessionId;
            else
                return sessionPlan.SessionId;
        }

        public void SetCompatibility(bool isCompatible)
        {
            background.color = isCompatible ? compatibleBackgroundColor : incompatibleBackgroundColor;
            
            incompatibleLabel.SetActive(!isCompatible);
        }
    }
}