using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.UI;
using UnityEngine.Events;

/// <summary>
/// A helper component that can allow another to start the content
/// marker flow without having the dependencies.
/// </summary>
public class StartContentMarkerComponent : BaseUiObject
{
    // Other references also use UnityEvents, so just sync it that way
    public UnityEvent startEvent;

    public UnityEvent cancelEvent;

    public UnityEvent finishEvent;

    private bool startedContentMarkerFlow = false;

    private IScenarioManager ScenarioManager { get; set; }

    [InjectDependencies]
    public void Construct(IScenarioManager scenarioManager)
    {
        ScenarioManager = scenarioManager;
    }

    /// <summary>
    /// Called from the Unity Editor.
    /// </summary>
    public void ToggleSettingContentMarker()
    {
        if (!startedContentMarkerFlow)
        {
            EventBus.Publish(new StartContentMarkerEvent(false));
        }
        else
        {
            ScenarioManager.AssetManager.SetContentMarker();
        }
    }

    public void CancelContentMarker()
    {
        if(startedContentMarkerFlow)
        {
            EventBus.Publish(new CancelContentMarkerEvent());
        }
    }

    public void ForceStart()
    {
        startedContentMarkerFlow = true;

        startEvent?.Invoke();
    }

    protected override void SubscribeToEventBuses()
    {
        EventBus.Subscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
        EventBus.Subscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        EventBus.Subscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
        EventBus.Unsubscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        EventBus.Unsubscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
    }

    private void OnCancelContentMarkerEvent(CancelContentMarkerEvent @event)
    {
        startedContentMarkerFlow = false;

        cancelEvent?.Invoke();
    }

    private void OnSetContentMarkerEvent(SetContentMarkerEvent @event)
    {
        startedContentMarkerFlow = false;

        finishEvent?.Invoke();
    }

    private void OnStartContentMarkerEvent(StartContentMarkerEvent @event)
    {
        startedContentMarkerFlow = true;

        startEvent?.Invoke();
    }
}
