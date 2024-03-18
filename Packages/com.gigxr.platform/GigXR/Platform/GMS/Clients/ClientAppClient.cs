using System;
using Cysharp.Threading.Tasks;
using GIGXR.GMS.Models;
using GIGXR.GMS.Models.Sessions;
using GIGXR.Platform.GMS;
using JetBrains.Annotations;

namespace GIGXR.GMS.Clients
{
    /// <summary>
    /// A client for accessing <c>/client-apps</c> endpoints from the GMS API.
    ///
    /// Don't use this class directly, prefer using <see cref="GmsClient"/>.
    /// </summary>
    public sealed class ClientAppClient
    {
        private readonly GmsWebRequestClient gmsWebRequestClient;
        private readonly GmsApiClientConfiguration configuration;

        public ClientAppClient(GmsApiClientConfiguration configuration, GmsWebRequestClient gmsWebRequestClient)
        {
            this.gmsWebRequestClient = gmsWebRequestClient;
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the <c>ClientAppManifest</c> from the API.
        /// </summary>
        /// <returns>The <c>ClientAppManifest</c>.</returns>
        [ItemCanBeNull]
        public async UniTask<ClientAppManifest> GetManifestAsync()
        {
            const string path = "client-apps/manifest";

            try
            {
                var response = await gmsWebRequestClient.Get<ClientAppManifestDetailedView>(path);
                // TODO Do we need this from UnityWebRequests? response.EnsureSuccessStatusCode();

                return response.Manifest.ToObject<ClientAppManifest>();
            }
            catch
            {
                // TODO Add back in
                //CloudLogger.LogError(exception);
                return null;
            }
        }

        // Added in version 1.1
        public async UniTask<string[]> GetVersionList()
        {
            string path = $"client-apps/{configuration.ClientAppId}/versions";

            return await gmsWebRequestClient.Get<string[]>(path, true);
        }

        // Added in version 1.1
        public async UniTask<string[]> GetAppVersions()
        {
            string path = "client-apps/versions";

            return await gmsWebRequestClient.Get<string[]>(path, true);
        }

        // Added in version 1.1
        public async UniTask<CreateClientAppVersionRequest> CreateClientAppVersionRequest(string version)
        {
            string path = "client-apps/versions";

            var request = new CreateClientAppVersionRequest(configuration.ClientAppId, version);

            return await gmsWebRequestClient.Post<CreateClientAppVersionRequest>(path, request.ToJsonString(), true);
        }
    }
}