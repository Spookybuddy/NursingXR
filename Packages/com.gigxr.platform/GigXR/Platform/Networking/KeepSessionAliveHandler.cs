using Cysharp.Threading.Tasks;
using GIGXR.GMS.Clients;
using GIGXR.Platform.Core;
using System;
using System.Threading;

namespace GIGXR.Platform.Networking
{
    /// <summary>
    /// Responsible for pinging the Session Status GMS Endpoint so that GMS knows the session is still active.
    /// </summary>
    public class KeepSessionAliveHandler : BaseBackgroundHandler
    {
        protected override int MillisecondsDelay { get; } = 60000;

        public SessionApiClient sessionClient;
        public Guid sessionId;

        public KeepSessionAliveHandler(SessionApiClient sessionClient, Guid sessionId)
        {
            this.sessionClient = sessionClient;
            this.sessionId = sessionId;
        }

        protected override async UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            await sessionClient.PingSessionAsync(sessionId);
        }
    }
}