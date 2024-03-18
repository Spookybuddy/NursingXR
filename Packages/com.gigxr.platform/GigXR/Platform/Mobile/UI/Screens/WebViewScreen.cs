namespace GIGXR.Platform.Mobile.UI
{
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
    using GIGXR.Platform.Mobile.AppEvents.Events.UI;
    using GIGXR.Platform.AppEvents.Events.Session;

    /// <summary>
    ///     The WebViewScreen contains components which wrap and control
    ///     a <see cref="UniWebView"/>, which displays the GMS website through
    ///     which users authenticate, view notifications, and select sessions
    ///     to join.
    /// </summary>
    public class WebViewScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.WebView;

        private INetworkManager NetworkManager { get; set; }

        [InjectDependencies]
        public void Construct(INetworkManager networkManager)
        {
            NetworkManager = networkManager;
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            NetworkManager.Subscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            NetworkManager.Unsubscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
        }

        private void OnLeftRoomNetworkEvent(LeftRoomNetworkEvent @event)
        {
            uiEventBus.Publish<SwitchingActiveScreenEventMobile>
            (
                new SwitchingActiveScreenEventMobile(ScreenTypeMobile.WebView, this.ScreenType)
            );
        }

        private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
        {
            uiEventBus.Publish<SwitchingActiveScreenEventMobile>
            (
                new SwitchingActiveScreenEventMobile(ScreenTypeMobile.WebView, this.ScreenType)
            );
        }
    }
}
