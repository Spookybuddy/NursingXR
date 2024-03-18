using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GIGXR.Platform.Mobile.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class UserList : MonoBehaviour
    {
        [SerializeField]
        private GameObject userListElementPrefab;

        private Dictionary<string, GameObject> instantiatedUserListElements = new Dictionary<string, GameObject>();

        private INetworkManager NetworkManager { get; set; }

        [InjectDependencies]
        public void Construct(INetworkManager networkManager)
        {
            NetworkManager = networkManager;

            NetworkManager.Subscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        private void OnApplicationQuit()
        {
            NetworkManager.Unsubscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
        }

        private void OnLeftRoomNetworkEvent(LeftRoomNetworkEvent @event)
        {
            ClearUserList();
        }

        private void OnPlayerEnteredRoomNetworkEvent(PlayerEnteredRoomNetworkEvent @event)
        {
            AddUser(@event.Player.UserId, @event.Player.NickName);
        }

        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            RemoveUser(@event.Player.UserId);
        }

        private void AddUser(string id, string name)
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());

            if(!instantiatedUserListElements.ContainsKey(id))
            {
                var userGameObject = Instantiate(userListElementPrefab, transform);
                instantiatedUserListElements.Add(id, userGameObject);

                userGameObject.GetComponent<TextMeshProUGUI>()?.SetText(name);
            }

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        private void RemoveUser(string id)
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());

            if (instantiatedUserListElements.ContainsKey(id))
            {
                Destroy(instantiatedUserListElements[id]);
                instantiatedUserListElements.Remove(id);
            }

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Called via UnityEditor when the 'Participants' button is pressed in the Mobile/Toolbar GameObject
        /// </summary>
        public void RefreshUserList()
        {
            ClearUserList();

            foreach (var currentPlayer in NetworkManager.AllPlayers)
            {
                AddUser(currentPlayer.UserId, currentPlayer.NickName);
            }
        }

        private void ClearUserList()
        {
            var childCopyList = instantiatedUserListElements.Values;

            foreach(GameObject currentChild in childCopyList)
            {
                Destroy(currentChild);
            }

            instantiatedUserListElements.Clear();
        }
    }
}