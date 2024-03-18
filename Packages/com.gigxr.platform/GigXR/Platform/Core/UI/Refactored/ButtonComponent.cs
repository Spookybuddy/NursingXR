using GIGXR.Platform.AppEvents.Events.UI.ButtonEvents;
using GIGXR.Platform.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

#if UNITY_WSA_10_0
using Microsoft.MixedReality.Toolkit;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(NearInteractionTouchable))]
[RequireComponent(typeof(PhysicalPressEventRouter))]
[RequireComponent(typeof(PressableButtonHoloLens2))]
#endif
public class ButtonComponent : UiObject
#if UNITY_WSA_10_0
    , IMixedRealitySpeechHandler
#endif
{
    private Interactable interactable;

    public delegate void EventHandler();

    public delegate void ButtonObjectClicked
    (
        ButtonComponent sender
    );

    public event ButtonObjectClicked OnClick;

    [SerializeField]
    public UnityEvent onClickEvents;

    [SerializeField]
    private string speechKeyword;

    [SerializeField]
    [Header("Going to make these obsolete soon, and use Quad with interactable themes instead. Do not use.")]
    private GameObject highlightObject;

    [SerializeField]
    private GameObject nonHighlightObject;

    [SerializeField]
    private TextMeshProUGUI buttonText;

#if UNITY_IOS || UNITY_ANDROID
    private Button button;
#endif

#if UNITY_WSA_10_0
    private PressableButtonHoloLens2 PressableButtonInfo
    {
        get
        {
            if (_pressableButtonInfo == null)
            {
                _pressableButtonInfo = GetComponent<PressableButtonHoloLens2>();
            }

            return _pressableButtonInfo;
        }
    }

    private PressableButtonHoloLens2 _pressableButtonInfo;

#endif

    protected virtual void Awake()
    {
        Initialize();
    }

    public void Highlight
    (
        bool highlight
    )
    {
        if (highlightObject == null)
        {
            return;
        }

        highlightObject.SetActive(highlight);

        if (nonHighlightObject)
            nonHighlightObject.SetActive(!highlight);
    }

    /// <summary>
    /// Set the UI object Collider enabled
    /// </summary>
    /// <param name="disable"></param>
    /// <param name="fade"></param>
    public override void IsDisabled(bool disable, bool fade = false)
    {
        base.IsDisabled(disable, fade);

#if UNITY_WSA_10_0
        if(PressableButtonInfo != null && PressableButtonInfo?.MovingButtonIconText != null)
            PressableButtonInfo.MovingButtonIconText.SetActive(!disable);
#endif
    }

    public override void Initialize()
    {
        base.Initialize();

#if UNITY_WSA_10_0
        if (GetComponent<Microsoft.MixedReality.Toolkit.UI.Interactable>())
        {
            interactable = GetComponent<Microsoft.MixedReality.Toolkit.UI.Interactable>();
            interactable.OnClick.AddListener(OnButtonClick);
        }
#endif
        // used to be else if here but I think we need both?
#if UNITY_IOS || UNITY_ANDROID
        if (GetComponent<Button>())
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }
#endif
    }

    public void SetInteractableTheme(InteractableStates.InteractableStateEnum state, bool stateValue)
    {
#if UNITY_WSA_10_0
        interactable.SetState(state, stateValue);
#endif
    }

    private void ToggleButtonState
    (
        SettingGlobalButtonStateEvent eventArgs
    )
    {
        base.IsDisabled(!eventArgs.setActive, eventArgs.fadeButton);
    }

    /// <summary>
    /// For editor tests.
    /// </summary>
#if UNITY_EDITOR
    private void OnMouseDown()
    {
        OnButtonClick();
    }
#endif

    // Currently used in UWP only. 
    public void TriggerClick()
    {
        OnButtonClick();
    }

    /// <summary>
    /// Adjusts the MRTK collider values that are scene when a pointer is near the object.
    /// </summary>
    /// <param name="value">The visual state of the collider</param>
    public void SetCompressableVisuals
    (
        bool value
    )
    {
#if UNITY_WSA_10_0
        PressableButtonInfo.CompressableButtonVisuals?.SetActive(value);
#endif
    }

    protected virtual void OnButtonClick()
    {
        onClickEvents?.Invoke();
        OnClick?.Invoke(this);
    }

    public void OnSpeechKeywordRecognized
    (
        SpeechEventData eventData
    )
    {
        if (eventData.Command.Keyword.ToLower() == speechKeyword.ToLower())
        {
            OnButtonClick();
        }
    }

    public void SetSpeechKeyword
    (
        string newSpeechCommand
    )
    {
        // If the previous speech keyword was blank, then we need to register with the InputSystem to receive callbacks
        if (speechKeyword == "")
        {
            RegisterSpeechHandler();
        }

        // Set the new speech keyword, the InputSystem is already providing the callbacks
        if (newSpeechCommand != "")
        {
            speechKeyword = newSpeechCommand;
        }
        // The new speech command is blank, so bring down the callbacks since the button does not need it anymore
        else
        {
            UnregisterSpeechHandler();
        }
    }

    public void SetButtonText(string newText)
    {
        if(buttonText != null)
        {
            buttonText.text = newText;
        }
        else
        {
            Debug.LogWarning($"Attempted to set new button text on {name} but buttonText is null. Text will remain the same.", this);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

#if UNITY_WSA_10_0
        RegisterSpeechHandler();
#endif
    }

    protected void RegisterSpeechHandler()
    {
#if UNITY_WSA_10_0
        if (speechKeyword != "" &&
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem != null)
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem
                .RegisterHandler<IMixedRealitySpeechHandler>(this);
        }
#endif
    }

    protected void UnregisterSpeechHandler()
    {
#if UNITY_WSA_10_0
        if (Microsoft.MixedReality.Toolkit.CoreServices.InputSystem != null)
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem
                .UnregisterHandler<IMixedRealitySpeechHandler>(this);
        }
#endif
    }

    protected override void SubscribeToEventBuses()
    {
        base.SubscribeToEventBuses();

        uiEventBus.Subscribe<SettingGlobalButtonStateEvent>(ToggleButtonState);
    }

    protected void OnDisable()
    {
#if UNITY_WSA_10_0
        UnregisterSpeechHandler();
#endif
    }

    protected void OnDestroy()
    {
        uiEventBus?.Unsubscribe<SettingGlobalButtonStateEvent>(ToggleButtonState);

#if UNITY_WSA_10_0
        if (interactable != null)
        {
            interactable.OnClick.RemoveListener(OnButtonClick);
        }
#endif
    }
}