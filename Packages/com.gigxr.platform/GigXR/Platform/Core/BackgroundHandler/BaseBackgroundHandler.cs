using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace GIGXR.Platform.Core
{
    /// <summary>
    /// A base class for creating a handler that runs an asynchronous task in the background.
    /// </summary>
    /// <remarks>
    /// Subclass and override <c>BackgroundTaskInternalAsync</c> with the desired background functionality.
    ///
    /// Optionally, override <c>MillisecondsDelay</c> to provide a custom delay between background executions.
    /// </remarks>
    public abstract class BaseBackgroundHandler : IDisposable
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }
        private bool Enabled { get; set; }

        protected virtual int MillisecondsDelay { get; } = 1000;

        public void Enable()
        {
            if (Enabled)
                return;

            Enabled = true;
            CancellationTokenSource = new CancellationTokenSource();
            _ = BackgroundTaskAsync(CancellationTokenSource.Token);
        }

        public void Disable()
        {
            if (!Enabled)
                return;

            Enabled = false;
            CancellationTokenSource.Cancel();
        }

        private async UniTask BackgroundTaskAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await BackgroundTaskInternalAsync(cancellationToken);
                await UniTask.Delay(MillisecondsDelay, false, PlayerLoopTiming.Update, cancellationToken);
            }

            CancellationTokenSource.Dispose();
        }

        protected abstract UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken);

        public void Dispose()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }
        }
    }
}