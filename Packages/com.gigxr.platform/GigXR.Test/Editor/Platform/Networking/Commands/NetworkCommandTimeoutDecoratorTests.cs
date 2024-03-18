using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Networking.Commands;
using GIGXR.Platform.Utilities;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace GIGXR.Test.Editor.Platform.Networking.Commands
{
    public class NetworkCommandTimeoutDecoratorTests
    {
        [UnityTest]
        public IEnumerator NetworkCommandTimeoutDecorator_RespectsNormalExecution()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(2);
            var command = new NetworkCommandTimeoutDecorator(new TestNetworkCommand(), timeout);

            // Act
            var task = command.ExecuteAsync();
            yield return new WaitForTimeSpan(TimeSpan.FromMilliseconds(100));

            // Assert
            Assert.IsFalse(task.Status.IsCanceled());
            Assert.IsTrue(task.Status.IsCompleted());
            // TODO Assert.DoesNotThrow(() => _ = task.Result);
            // TODO Assert.IsTrue(task.Result);
        }

        [UnityTest]
        public IEnumerator NetworkCommandTimeoutDecorator_CancelsLongRunningTask()
        {
            // Arrange
            var timeout = TimeSpan.FromMilliseconds(5);
            var command = new NetworkCommandTimeoutDecorator(new LongRunningTestNetworkCommand(), timeout);

            // Act
            var task = command.ExecuteAsync();
            yield return new WaitForTimeSpan(TimeSpan.FromMilliseconds(100));

            // Assert
            Assert.IsTrue(task.Status.IsCanceled());
            Assert.IsTrue(task.Status.IsCompleted());
            // TODO Assert.Throws<AggregateException>(() => _ = task.Result);
        }

        private class TestNetworkCommand : INetworkCommand
        {
            public UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
            {
                return UniTask.FromResult(true);
            }
        }

        private class LongRunningTestNetworkCommand : INetworkCommand
        {
            public async UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
            {
                await UniTask.Delay((int)TimeSpan.FromSeconds(10).TotalMilliseconds, true, PlayerLoopTiming.Update, cancellationToken);
                return true;
            }
        }
    }
}