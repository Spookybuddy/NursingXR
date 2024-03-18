using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

public enum ArgsType
{
    String,
    Int,
    Float,
    Bool,
    Guid
}

#if UNITY_WSA_10_0
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(NearInteractionTouchable))]
[RequireComponent(typeof(PhysicalPressEventRouter))]
[RequireComponent(typeof(PressableButtonHoloLens2))]
#endif
public class ButtonObjectWithArgs : ButtonComponent
{
    public delegate void ButtonClickedEventStringHandler(string stringArgs);

    public delegate void ButtonClickedEventStringAndSenderHandler
        (ButtonObjectWithArgs sender, string stringArgs);

    public delegate void ButtonClickedEventBoolHandler(bool boolArgs);

    public delegate void ButtonClickedEventIntHandler(int intArgs);

    public delegate void ButtonClickedEventFloatHandler(float floatArgs);

    public delegate void ButtonClickedEventGuidHandler(Guid guidArgs);

    public event ButtonClickedEventStringHandler OnButtonClickedString;
    public event ButtonClickedEventStringAndSenderHandler OnButtonClickedStringAndSender;
    public event ButtonClickedEventBoolHandler OnButtonClickedBool;
    public event ButtonClickedEventIntHandler OnButtonClickedInt;
    public event ButtonClickedEventFloatHandler OnButtonClickedFloat;
    public event ButtonClickedEventGuidHandler OnButtonClickedGuid;

    [SerializeField] private ArgsType argsType;

    private Args args;

    public Args GetArgs { get { return args; } }

    /// <summary>
    /// Sets the args type.
    /// </summary>
    /// <param name="type"></param>
    public void SetArgsType(ArgsType type)
    {
        argsType = type;
    }

    /// <summary>
    /// Sets the property to be passed on this button's OnClick event.
    /// </summary>
    /// <param name="newArgs"></param>
    public void SetArgs(Args newArgs)
    {
        args = newArgs;
    }

    /// <summary>
    /// Called from the ButtonComponent's click event.
    /// </summary>
    protected override void OnButtonClick()
    {
        base.OnButtonClick();

        switch (argsType)
        {
            case ArgsType.String:
                OnButtonClickedString?.Invoke(args.stringArgs);
                OnButtonClickedStringAndSender?.Invoke(this, args.stringArgs);
                break;
            case ArgsType.Int:
                OnButtonClickedInt?.Invoke(args.intArgs);
                break;
            case ArgsType.Float:
                OnButtonClickedFloat?.Invoke(args.floatArgs);
                break;
            case ArgsType.Bool:
                OnButtonClickedBool?.Invoke(args.boolArgs);
                break;
            case ArgsType.Guid:
                OnButtonClickedGuid?.Invoke(args.guidArgs);
                break;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Copy GUID")]
    public void CopyGUID()
    {
        UnityEditor.EditorGUIUtility.systemCopyBuffer = args.guidArgs.ToString();
    }
#endif

    [Serializable]
    public class Args
    {
        public string stringArgs;
        public int    intArgs;
        public float  floatArgs;
        public bool   boolArgs;
        public Guid   guidArgs;

        public Args(string args)
        {
            stringArgs = args;
        }

        public Args(int args)
        {
            intArgs = args;
        }

        public Args(float args)
        {
            floatArgs = args;
        }

        public Args(bool args)
        {
            boolArgs = args;
        }

        public Args(Guid args)
        {
            guidArgs = args;
        }
    }
}