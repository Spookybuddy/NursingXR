using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    /// <summary>
    /// https://forum.photonengine.com/discussion/comment/45825
    /// </summary>
    public class ReconnectAndRejoinNetworkCommand : BaseNetworkCommand
    {
        protected override UniTask<bool> ExecuteInternalAsync()
        {
            Debug.Log("RAR: Reconnect And Rejoin (RAR) started.");

            if (!ReconnectAndRejoin())
            {
                Debug.Log("RAR: Rejoin directly to room failed, trying to reconnect.");

                if (!Reconnect())
                {
                    Debug.Log("RAR: Reconnect failed!");
                    return UniTask.FromResult(false);
                }
                else
                {
                    Debug.Log("RAR: Reconnect success!");
                }
            }
            else
            {
                Debug.Log("RAR: Rejoin directly to room was a success.");
            }

            return Promise.Task;
        }

        public override void OnJoinedRoom()
        {
            // Able to join directly to the previous room.
            Promise.TrySetResult(true);
        }

        public override void OnConnectedToMaster()
        {
            // Could not join the room, but could reconnect to the master server.
            Promise.TrySetResult(true);
        }

        protected virtual bool ReconnectAndRejoin() => PhotonNetwork.ReconnectAndRejoin();

        protected virtual bool Reconnect() => PhotonNetwork.Reconnect();
    }
}