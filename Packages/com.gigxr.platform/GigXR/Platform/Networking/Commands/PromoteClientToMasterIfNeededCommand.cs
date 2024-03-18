using GIGXR.Platform.Networking.EventBus.Events;
using Photon.Realtime;
using System;

namespace GIGXR.Platform.Networking.Commands
{
    internal class PromoteClientToMasterIfNeededCommand
    {
        protected Player Client { get; }

        protected INetworkManager NetworkManager { get; }

        public PromoteClientToMasterIfNeededCommand(Player client, INetworkManager networkManager)
        {
            Client = client;
            NetworkManager = networkManager;
        }

        public bool Execute()
        {
            if (!NetworkManager.IsMasterClient || NetworkManager.CurrentRoom.CustomProperties == null)
                return false;

            var ownerId = NetworkManager.CurrentRoom.CustomProperties.GetOwnerId();

            if (ownerId == Guid.Empty)
                return false;

            if (Client.UserId == ownerId.ToString())
            {
                // This event raised via the NetworkManager will set a new MasterClient via PhotonNetwork while
                // also making sure all users are also kept in sync with who the new host is and their UIs matching
                NetworkManager.RaiseNetworkEvent(new PromoteToHostNetworkEvent(ownerId));
            }

            return true;
        }
    }
}