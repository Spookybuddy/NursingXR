namespace GIGXR.Platform.Networking.Commands
{
    using Cysharp.Threading.Tasks;
    using System.Threading;

    public interface INetworkCommand
    {
        /// <summary>
        /// Executes the network command asynchronously and returns a task that represents if the operation succeeded.
        /// </summary>
        /// <param name="cancellationToken">An optional CancellationToken to cancel this command.</param>
        /// <returns>Whether the operation succeeded.</returns>
        UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}