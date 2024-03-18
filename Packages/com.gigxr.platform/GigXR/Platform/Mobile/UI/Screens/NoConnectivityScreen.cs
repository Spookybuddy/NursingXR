namespace GIGXR.Platform.Mobile.UI
{
    using GIGXR.Platform.Mobile.AppEvents.Events.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Mobile;
    using GIGXR.Platform.Sessions;
    using GIGXR.Platform.Networking;

    /// <summary>
    ///     The NoConnectivityScreen exists to notify users when they
    ///     have lost connectivity. It is controlled by the
    ///     <see cref="ConnectivityManager"/>.
    /// </summary>
    public class NoConnectivityScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.Connectivity;

        private INetworkManager _networkManager;

        [InjectDependencies]
        public void Construct(INetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void Show(bool hasConnectivity)
        {
            if (hasConnectivity)
            {
                // If we are connected, then we're either in session we are not in session.
                // If we are in a session, we should show the Session screen.
                // Otherwise, back to WebView
                if (_networkManager.InRoom)
                {
                    uiEventBus.Publish<SwitchingActiveScreenEventMobile>
                    (
                        new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Session, this.ScreenType)
                    );
                }
                else
                {
                    uiEventBus.Publish<SwitchingActiveScreenEventMobile>
                    (
                        new SwitchingActiveScreenEventMobile(ScreenTypeMobile.WebView, this.ScreenType)
                    );
                }
            }
            else
            {
                // If we are not connected, No Connectivity Screen!
                uiEventBus.Publish<SwitchingActiveScreenEventMobile>
                (
                    new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Connectivity, this.ScreenType)
                );
            }
        }
    }
}
