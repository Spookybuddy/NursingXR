using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Networking.Commands;
using GIGXR.Platform.Utilities;
using NUnit.Framework;
using Photon.Realtime;
using UnityEngine.TestTools;


namespace GIGXR.Test.Editor.Platform.Networking.Commands
{
    public class DisconnectNetworkCommandTests
    {
        [UnityTest]
        public IEnumerator DisconnectNetworkCommand_ReturnsTrueForAlreadyDisconnectedState() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableDisconnectNetworkCommand { TestNetworkClientState = ClientState.Disconnected };

            // Act
            var taskResult = await command.ExecuteAsync();

            // Assert
            Assert.IsTrue(taskResult);
        });

        [UnityTest]
        public IEnumerator DisconnectNetworkCommand_ReturnsTrueForInitialState() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableDisconnectNetworkCommand { TestNetworkClientState = ClientState.PeerCreated };

            // Act
            var taskResult = await command.ExecuteAsync();

            // Assert
            Assert.IsTrue(taskResult);
        });

        [UnityTest]
        public IEnumerator DisconnectNetworkCommand_RespectsCancellationToken() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableDisconnectNetworkCommand { TestNetworkClientState = ClientState.Disconnecting };
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = command.ExecuteAsync(cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            // Assert
            Assert.IsTrue(task.Status.IsCompleted());
            // TODO Assert.IsFalse(task);
        });

        private class TestableDisconnectNetworkCommand : DisconnectNetworkCommand
        {
            public ClientState TestNetworkClientState { get; set; }

            protected override ClientState NetworkClientState => TestNetworkClientState;

            protected override void Disconnect() => OnDisconnected(DisconnectCause.DisconnectByClientLogic);

            protected override void AddCallbackTarget()
            {
            }

            protected override void RemoveCallbackTarget()
            {
            }
        }
    }
}