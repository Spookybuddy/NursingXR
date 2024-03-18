using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Networking.Commands;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace GIGXR.Test.Editor.Platform.Networking.Commands
{
    public class JoinRoomNetworkCommandTests
    {
        [UnityTest]
        public IEnumerator JoinRoomNetworkCommand_ReturnsFalseIfPhotonJoinRoomReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableJoinRoomNetworkCommand("TestRoom") { TestJoinRoomResult = false };

            // Act
            var taskResult = await command.ExecuteAsync();

            // Assert
            Assert.IsFalse(taskResult);
        });

        [UnityTest]
        public IEnumerator JoinRoomNetworkCommand_ReturnsTrueIfOnJoinedRoomIsCalled() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableJoinRoomNetworkCommand("TestRoom") { TestJoinRoomResult = true };

            // Act
            var task = command.ExecuteAsync();
            command.OnJoinedRoom();
            var taskResult = await task;

            // Assert
            Assert.IsTrue(taskResult);
        });

        [UnityTest]
        public IEnumerator JoinRoomNetworkCommand_RespectsCancellationToken() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableJoinRoomNetworkCommand("TestRoom") { TestJoinRoomResult = true };
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = command.ExecuteAsync(cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
            
            var taskResult = await task;

            // Assert
            Assert.IsTrue(task.Status.IsCompleted());
            Assert.IsFalse(taskResult);
        });

        private class TestableJoinRoomNetworkCommand : JoinRoomNetworkCommand
        {
            public bool TestJoinRoomResult { get; set; }

            public TestableJoinRoomNetworkCommand(string roomName) : base(roomName)
            {
            }

            protected override bool JoinRoom(string roomName)
            {
                return TestJoinRoomResult;
            }

            protected override void AddCallbackTarget()
            {
            }

            protected override void RemoveCallbackTarget()
            {
            }
        }
    }
}