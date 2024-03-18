namespace GIGXR.GMS.Clients
{
    using GIGXR.Platform;
    using GIGXR.Platform.AppEvents;
    using System;

    /// <summary>
    /// An aggregate client class that sets up and references multiple clients for communicating with GMS at different endpoints 
    /// such as sessions and accounts.
    /// </summary>
    public class GmsApiClient
    {
        public GmsApiClient(AppEventBus eventBus, GmsApiClientConfiguration configuration, ProfileManager profileManager, GmsWebRequestClient gmsWebRequestClient)
        {
            if (configuration == null)
                throw new ArgumentException("configuration cannot be null!", nameof(configuration));
            
            AccountsApi = new AccountApiClient(eventBus, configuration, gmsWebRequestClient, profileManager);
            ClientApps = new ClientAppClient(configuration, gmsWebRequestClient);
            SessionsApi = new SessionApiClient(configuration, gmsWebRequestClient, profileManager);
            ProfileManager = profileManager;
        }

        public GmsApiClient(AppEventBus eventBus, GmsApiClientConfiguration configuration, ProfileManager profileManager) : 
            this(eventBus, configuration, profileManager, new GmsWebRequestClient(configuration))
        {
        }

        public AccountApiClient AccountsApi { get; }
        public ClientAppClient ClientApps { get; }
        public SessionApiClient SessionsApi { get; }
        public ProfileManager ProfileManager { get; }
    }
}