namespace GIGXR.Sandbox.UI.Screens
{
    using GMS.Clients;
    using Platform.Core.DependencyInjection;
    using UnityEngine;

    public class ScreenController : MonoBehaviour
    {
        private DemoAuthenticationScreen authenticationScreen;
        private DemoSessionsScreen sessionsScreen;
        private GmsApiClient GmsApiClient { get; set; }

        private void Awake()
        {
            authenticationScreen = GetComponent<DemoAuthenticationScreen>();
            sessionsScreen = GetComponent<DemoSessionsScreen>();
        }

        private void Update()
        {
            if (GmsApiClient.AccountsApi.AuthenticatedAccount == null)
            {
                authenticationScreen.Show();
                sessionsScreen.Hide();
            }
            else
            {
                authenticationScreen.Hide();
                sessionsScreen.Show();
            }
        }

        [InjectDependencies]
        public void Construct(GmsApiClient gmsApiClient)
        {
            GmsApiClient = gmsApiClient;
        }
    }
}