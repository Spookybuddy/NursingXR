namespace GIGXR.Sandbox.UI.Screens
{
    using GMS.Clients;
    using Platform.Core.DependencyInjection;
    using UnityEngine;

    public class DemoAuthenticationScreen : MonoBehaviour
    {
        [SerializeField]
        private bool isHidden;
        
        private Rect windowRect = new Rect(20, 20, 200, 50);
        private string email = "";
        private string password = "";

        private GmsApiClient GmsApiClient { get; set; }

        [InjectDependencies]
        public void Construct(GmsApiClient gmsApiClient)
        {
            GmsApiClient = gmsApiClient;
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
            
            windowRect = GUILayout.Window(0, windowRect, SetupWindow, "Authentication");
        }

        private void SetupWindow(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Email");
            email = GUILayout.TextField(email);

            GUILayout.Label("Password");
            password = GUILayout.PasswordField(password, '*');

            if (GUILayout.Button("Login"))
            {
                GmsApiClient.AccountsApi.LoginWithEmailPassAsync(email, password);
            }

            GUILayout.EndVertical();
        }
    }
}