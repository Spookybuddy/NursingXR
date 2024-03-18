using System;
using System.Collections.Generic;
using System.Net.Http;
using GIGXR.GMS.Models;
using GIGXR.GMS.Models.Sessions;
using Newtonsoft.Json;

namespace GIGXR.GMS.Clients
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform;
    using Models.Sessions.Requests;
    using Models.Sessions.Responses;
    using Platform.Data;
    using Platform.GMS;

    /// <summary>
    /// A client for accessing <c>/sessions</c> endpoints from the GMS API.
    ///
    /// Don't use this class directly, prefer using <see cref="GmsClient"/>.
    /// </summary>
    public sealed partial class SessionApiClient : BaseApiClient
    {
        private readonly GmsApiClientConfiguration configuration;
        private readonly GmsWebRequestClient gmsWebRequestClient;
        private readonly ProfileManager profileManager;

        public SessionApiClient(GmsApiClientConfiguration configuration, GmsWebRequestClient gmsWebRequestClient, ProfileManager profileManager)
        {
            this.configuration = configuration;
            this.gmsWebRequestClient = gmsWebRequestClient;
            this.profileManager = profileManager;
        }

        /// <summary>
        /// Determines if the given user owns the given session
        /// </summary>
        /// <param name="sessionId">The session ID of interest</param>
        /// <param name="userId">The user ID of interest</param>
        /// <returns>True if the user created this session, otherwise false</returns>
        public async UniTask<bool> IsSessionOwner(Guid sessionId, Guid userId)
        {
            var session = await GetSessionAsync(sessionId);

            return session.CreatedBy.AccountId == userId;
        }

        /// <summary>
        /// Returns a list of all the Active Sessions in GMS
        /// </summary>
        /// <returns>A list of all active sessions in GMS with their partial session data</returns>
        public async UniTask<List<SessionListView>> GetActiveSessionListAsync()
        {
            const string path = "sessions/available";

            return await gmsWebRequestClient.Get<List<SessionListView>>(path, true);
        }

        /// <summary>
        /// Returns a list of all the Saved Sessions in GMS
        /// </summary>
        /// <returns>A list of all saved sessions in GMS with their partial session data</returns>
        public async UniTask<List<SessionListView>> GetSavedSessionsAsync()
        {
            const string path = "sessions/saved";
            var savedSessions = await gmsWebRequestClient.Get<List<SessionListView>>(path, true);
            return savedSessions;
        }

        /// <summary>
        /// Updates the Session endpoint with whatever values are included in the PatchSessionRequest.
        /// </summary>
        /// <param name="request">The DTO holding the session values to update</param>
        public async UniTask PatchSessionAsync(ICollection<PatchSessionRequest> request)
        {
            const string path = "sessions";

            var serializedContent = JsonConvert.SerializeObject(request,
                                                Formatting.None,
                                                new JsonSerializerSettings()
                                                {
                                                    NullValueHandling = NullValueHandling.Ignore
                                                });

            // The Patch API endpoint on 1.1 is throwing an error when used despite the content looking correct
            // so we are forcing the endpoint to not use versioning here regardless of what is set
            await gmsWebRequestClient.Patch(path, serializedContent, false);
        }

        public async UniTask PatchSessionPlansAsync(ICollection<PatchSessionRequest> request)
        {
            const string path = "sessions/plans";

            var serializedContent = JsonConvert.SerializeObject(request,
                                                Formatting.None,
                                                new JsonSerializerSettings()
                                                {
                                                    NullValueHandling = NullValueHandling.Ignore
                                                });

            // The Patch API endpoint on 1.1 is throwing an error when used despite the content looking correct
            // so we are forcing the endpoint to not use versioning here regardless of what is set
            await gmsWebRequestClient.Patch(path, serializedContent, false);
        }

        /// <summary>
        /// Creates a session with the configuration passed in the <see cref="CreateSessionFromSourceSessionRequest"/>.
        /// </summary>
        /// <param name="request">The data needed to create the session</param>
        /// /// <param name="clientAppVersion">Optional, used if the session was created from another session.</param>
        /// <returns>The complete session data for the session that was just created</returns>
        public async UniTask<SessionDetailedView> CreateSessionAsync(CreateSessionRequest request, string clientAppVersion = null)
        {
            const string path = "sessions";

            // When creating a new session with versioning, it's required to include the version string. We assume that
            // callers do have the version set in the DTO, so handle it at the client level.
            if(configuration.GmsVersion != null)
            {
                if(string.IsNullOrEmpty(clientAppVersion))
                {
                    request.ClientAppVersion = profileManager.appDetailsProfile.appDetailsScriptableObject.VersionString;
                }
                else
                {
                    request.ClientAppVersion = clientAppVersion;
                }
            }

            return await gmsWebRequestClient.Post<SessionDetailedView>(path, request.ToJsonString(), true, false);
        }

        /// <summary>
        /// Creates a session plan with the configuration passed in the <see cref="CreateSessionFromSourceSessionRequest"/>.
        /// </summary>
        /// <param name="request">The data needed to create the session plan</param>
        /// <returns>The complete session data for the session that was just created</returns>
        public async UniTask<SessionDetailedView> CreateSessionPlanAsync(CreateSessionPlanRequest request)
        {
            const string path = "sessions/plans";

            // When creating a new session with versioning, it's required to include the version string. We assume that
            // callers do have the version set in the DTO, so handle it at the client level.
            if (configuration.GmsVersion != null)
            {
                request.ClientAppVersion = profileManager.appDetailsProfile.appDetailsScriptableObject.VersionString;
            }

            return await gmsWebRequestClient.Post<SessionDetailedView>(path, request.ToJsonString(), true);
        }

        /// <summary>
        /// Gets the complete session data for a particular session
        /// </summary>
        /// <param name="sessionId">The ID of the session of interest</param>
        /// <returns>All session data points for a particular session</returns>
        public async UniTask<SessionDetailedView> GetSessionAsync(Guid sessionId)
        {
            var path = $"sessions/{sessionId}";

            return await gmsWebRequestClient.Get<SessionDetailedView>(path, true);
        }

        /// <summary>
        /// Updates a specific session's data
        /// </summary>
        /// <param name="sessionId">The session ID to update</param>
        /// <param name="request">The new session data</param>
        /// <returns>The complete session data object</returns>
        public async UniTask<SessionDetailedView> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request)
        {
            var path = $"sessions/{sessionId}";

            // Versioning not implemented at this endpoint yet
            return await gmsWebRequestClient.Put<SessionDetailedView>(path, request.ToJsonString());
        }

        /// <summary>
        /// Retrieves the participation status of a particular user.
        /// </summary>
        /// <param name="sessionId">The session ID the user is part of</param>
        /// <param name="accountId">The ID of the user of interest</param>
        /// <returns>The account detail of the given user with their session status</returns>
        public async UniTask<SessionParticipantDetailedView> GetParticipantStatusAsync(Guid sessionId, Guid accountId)
        {
            var path = $"sessions/{sessionId}/participants/{accountId}";

            return await gmsWebRequestClient.Get<SessionParticipantDetailedView>(path);
        }

        /// <summary>
        /// Updates a specific user's session participation status for a specific session
        /// </summary>
        /// <param name="sessionId">The ID of the session the user needs to update</param>
        /// <param name="accountId">The ID of the user themselves</param>
        /// <param name="status">The new status for the participant in this session</param>
        public async UniTask UpdateParticipantStatusAsync(Guid sessionId, Guid accountId, SessionParticipantStatus status)
        {
            var path = $"sessions/{sessionId}/participants/{accountId}";
            var request = new UpdateSessionParticipantRequest { SessionParticipantStatus = status };

            await gmsWebRequestClient.Put(path, request.ToJsonString());
        }

        /// <summary>
        /// Pings the Session on GMS so that it remains active on the GMS side
        /// </summary>
        /// <param name="sessionId">The Session ID to ping</param>
        public async UniTask PingSessionAsync(Guid sessionId)
        {
            var path = $"sessions/{sessionId}/status";
            await gmsWebRequestClient.Post(path, null);
        }

        /// <summary>
        /// Gets all the session plans that are available 
        /// </summary>
        /// <returns>A list of all the session plans and their partial data</returns>
        public async UniTask<List<SessionPlanListView>> GetSessionPlansAsync()
        {
            const string path = "sessions/plans";
            
            return await gmsWebRequestClient.Get<List<SessionPlanListView>>(path, true);
        }

        /// <summary>
        /// Gets the complete data for a Session Plan
        /// </summary>
        /// <param name="sessionId">The Session Plan ID that needs to be retrieved</param>
        /// <returns>The complete session plan data for the given ID</returns>
        public async UniTask<SessionDetailedView> GetSessionPlanAsync(Guid sessionId)
        {
            string path = $"sessions/plans/{sessionId}";

            return await gmsWebRequestClient.Get<SessionDetailedView>(path, true, false);
        }

        /// <summary>
        /// Updates a specific session plan's data
        /// </summary>
        /// <param name="sessionId">The Session Plan ID that needs to be updated</param>
        /// <param name="request">The new session plan data</param>
        /// <returns>The complete session plan data with the updated value</returns>
        public async UniTask<SessionDetailedView> UpdateSessionPlanAsync(Guid sessionId, UpdateSessionPlanRequest request)
        {
            var path = $"sessions/plans/{sessionId}";

            // Versioning not implemented at this endpoint yet
            return await gmsWebRequestClient.Put<SessionDetailedView>(path, request.ToJsonString());
        }

        /// <summary>
        /// Locks a session so that other users cannot join it
        /// </summary>
        /// <param name="sessionId">The ID of the session to lock</param>
        /// <param name="request">The data that specifies the lock status of the session</param>
        public async UniTask SetSessionLockAsync(Guid sessionId, UpdateSessionLockRequest request)
        {
            var sessionData = await GetSessionAsync(sessionId);
            sessionData.Locked = request.Locked;

            await UpdateSessionAsync(sessionId, new UpdateSessionRequest(sessionData));
        }

        /// <summary>
        /// Locks a session so that other users cannot join it
        /// </summary>
        /// <param name="sessionId">The ID of the session to lock</param>
        /// <param name="request">The data that specifies the lock status of the session</param>
        public async UniTask SetSessionPermissionAsync(Guid sessionId, UpdateSessionPermissionRequest request, SessionDetailedView sessionDetails = null)
        {
            if(sessionDetails == null)
            {
                sessionDetails = await GetSessionAsync(sessionId);
            }

            sessionDetails.SessionPermission = request.Permission;

            await UpdateSessionAsync(sessionId, new UpdateSessionRequest(sessionDetails));
        }

        /// <summary>
        /// Creates a temporary data endpoint so that in-session data can be saved and modified by all users for a given session
        /// </summary>
        /// <param name="sessionEphemeralDataId">The ID that will reference this specific endpoint</param>
        /// <param name="request">The data that will be stored at the endpoint</param>
        /// <returns>True if the data was saved successfully, otherwise false</returns>
        public async UniTask CreateEphemeralDataAsync(Guid sessionEphemeralDataId, CreateEphemeralDataRequest request)
        {
            var path = $"sessions/ephemeral-data/{sessionEphemeralDataId}";

            await gmsWebRequestClient.Put(path, request.ToJsonString());
        }

        /// <summary>
        /// Gets the data that is stored at a particular ephemeral end point
        /// </summary>
        /// <param name="sessionEphemeralDataId"></param>
        /// <returns></returns>
        public async UniTask<EphemeralDataView> GetEphemeralDataAsync(Guid sessionEphemeralDataId)
        {
            var path = $"sessions/ephemeral-data/{sessionEphemeralDataId}";

            return await gmsWebRequestClient.Get<EphemeralDataView>(path);
        }
    }

    //
    // TODO: Code below needs to be updated. It is platform 2 legacy.
    //
    public sealed partial class SessionApiClient
    {
        public async UniTask<List<SessionResourceView>> GetSessionResourcesAsync()
        {
            try
            {
                const string path = "sessions/resources";

                var response =  await gmsWebRequestClient.Get<string>(path);

                var view = JsonConvert.DeserializeObject<SuccessResponse<List<SessionResourceView>>>(response);

                return view.data;
            }
            catch (Exception)
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                return new List<SessionResourceView>();
            }
        }
    }
}