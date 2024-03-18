using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace GIGXR.Platform.Networking.Commands
{
    /// <summary>
    /// A decorator for <c>INetworkCommand</c> that adds timeout functionality.
    /// </summary>
    public class NetworkCommandTimeoutDecorator : INetworkCommand
    {
        private INetworkCommand NetworkCommand { get; }
        private TimeSpan Timeout { get; }

        public NetworkCommandTimeoutDecorator(INetworkCommand networkCommand, TimeSpan timeout)
        {
            NetworkCommand = networkCommand;
            Timeout = timeout;
        }

        public UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(Timeout);

            return NetworkCommand.ExecuteAsync(cancellationTokenSource.Token);
        }
    }
}