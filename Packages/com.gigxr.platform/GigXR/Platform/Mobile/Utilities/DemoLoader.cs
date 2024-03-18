using Cysharp.Threading.Tasks;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.Mobile.WebView;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;
using GIGXR.Platform.Utilities;
using UnityEngine;

[RequireComponent(typeof(WebViewController))]
public class DemoLoader : MonoBehaviour
{
    public GameObject demoPrefab;

    private WebViewController webViewController;

    private GameObject instantiatedAsset;

    private AppEventBus EventBus;

    private ICalibrationManager CalibrationManager;

    private ICalibrationRootProvider CalibrationRoot;

    [InjectDependencies]
    public async void Construct(AppEventBus bus, ICalibrationManager calibrationManager, ICalibrationRootProvider calibrationRoot)
    {
        EventBus = bus;
        CalibrationManager = calibrationManager;
        CalibrationRoot = calibrationRoot;

        webViewController = GetComponent<WebViewController>();

        if (webViewController.WebViewEventBus == null)
            await UniTask.WaitUntil(() => webViewController.WebViewEventBus != null);

        webViewController.WebViewEventBus.Subscribe<JoinDemoSessionWebViewToUnityEvent>(OnJoinDemoSessionWebViewToUnityEvent);

        EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
        EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
    }

    private void OnDestroy()
    {
        webViewController.WebViewEventBus.Unsubscribe<JoinDemoSessionWebViewToUnityEvent>(OnJoinDemoSessionWebViewToUnityEvent);

        EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
        EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
    }

    // Clean up assets
    private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
    {
        if(instantiatedAsset != null)
        {
            Destroy(instantiatedAsset);
        }
    }

    private void OnJoinDemoSessionWebViewToUnityEvent(JoinDemoSessionWebViewToUnityEvent @event)
    {
        CalibrationManager.StartCalibration(ICalibrationManager.CalibrationModes.Manual);

        if(demoPrefab != null)
        {
            instantiatedAsset = Instantiate(demoPrefab, CalibrationRoot.AnchorRoot);
            instantiatedAsset.SetActive(false);
        }
    }

    // Show assets after calibration
    private void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
    {
        if(instantiatedAsset != null)
        {
            instantiatedAsset.SetActive(true);
        }  
    }
}
