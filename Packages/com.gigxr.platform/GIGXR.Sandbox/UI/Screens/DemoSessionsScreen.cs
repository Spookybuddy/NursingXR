namespace GIGXR.Sandbox.UI.Screens
{
    using GMS.Clients;
    using Platform.Core.DependencyInjection;
    using Platform.Sessions;
    using UnityEngine;

    public class DemoSessionsScreen : MonoBehaviour
    {
        [SerializeField]
        private bool isHidden;
        
        private Rect windowRect = new Rect(20, 20, 200, 50);

        private GmsApiClient GmsApiClient { get; set; }
        private ISessionManager SessionManager { get; set; }

        [InjectDependencies]
        public void Construct(GmsApiClient gmsApiClient, ISessionManager sessionManager)
        {
            GmsApiClient = gmsApiClient;
            SessionManager = sessionManager;
        }

        public void Hide()
        {
            isHidden = true;
        }

        public void Show()
        {
            isHidden = false;
        }

        private void OnGUI()
        {
            if (isHidden)
            {
                return;
            }
            
            windowRect = GUILayout.Window(0, windowRect, SetupWindow, "Sessions");
        }

        private void SetupWindow(int windowId)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Logout"))
            {
                GmsApiClient.AccountsApi.LogoutAsync();
            }

            GUILayout.EndVertical();
        }
    }
}