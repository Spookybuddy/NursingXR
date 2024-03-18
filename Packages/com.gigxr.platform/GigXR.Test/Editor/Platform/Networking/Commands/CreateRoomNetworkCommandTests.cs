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
    public class CreateRoomNetworkCommandTests
    {
        [UnityTest]
        public IEnumerator CreateRoomNetworkCommand_ReturnsFalseIfPhotonCreateRoomReturnsFalse() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableCreateRoomNetworkCommand("TestRoom", new RoomOptions())
            {
                TestCreateRoomResult = false
            };

            // Act
            var task = await command.ExecuteAsync();

            // Assert
            Assert.IsFalse(task);
        });

        [UnityTest]
        public IEnumerator CreateRoomNetworkCommand_ReturnsTrueIfOnCreatedRoomIsCalled() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableCreateRoomNetworkCommand("TestRoom", new RoomOptions())
            {
                TestCreateRoomResult = true
            };

            // Act
            var task = command.ExecuteAsync();
            command.OnCreatedRoom();
            var taskResult = await task;

            // Assert
            Assert.IsTrue(taskResult);
        });

        [UnityTest]
        public IEnumerator CreateRoomNetworkCommand_ReturnsFalseIfOnCreateRoomFailedCalled() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableCreateRoomNetworkCommand("TestRoom", new RoomOptions())
            {
                TestCreateRoomResult = true
            };
            const int returnCodeNoOp = 0;
            const string messageNoOp = "";

            // Act
            var task = command.ExecuteAsync();
            command.OnCreateRoomFailed(returnCodeNoOp, messageNoOp);
            var taskResult = await task;

            // Assert
            Assert.IsFalse(taskResult);
        });

        [UnityTest]
        public IEnumerator CreateRoomNetworkCommand_RespectsCancellationToken() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var command = new TestableCreateRoomNetworkCommand("TestRoom", new RoomOptions())
            {
                TestCreateRoomResult = false
            };
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = command.ExecuteAsync(cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
            await UniTask.Delay(TimeSpan.FromMilliseconds(50));

            // Assert
            Assert.IsTrue(task.Status.IsCompleted());
            // TODO Assert.IsFalse(task.Result);
        });

        private class TestableCreateRoomNetworkCommand : CreateRoomNetworkCommand
        {
            public bool TestCreateRoomResult { get; set; }

            public TestableCreateRoomNetworkCommand(string roomName, RoomOptions roomOptions)
                : base(roomName, roomOptions)
            {
            }

            protected override bool CreateRoom(string roomName, RoomOptions roomOptions)
            {
                return TestCreateRoomResult;
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