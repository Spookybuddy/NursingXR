using UnityEngine;
using UnityEngine.Events;

public class ToggleButtonComponent : ButtonComponent
{
    [SerializeField] private bool initiallyOn;
    [SerializeField] private bool invokeToggleOnIntialize;
    [SerializeField] private bool supressToggleOffOnClick = false;
    [SerializeField] private bool manageToggleExternallyOnAllClicks = false;

    public bool SupressToggleOffOnClick
    {
        get => supressToggleOffOnClick;
        set => supressToggleOffOnClick = value;
    }

    private bool on;
    public bool CurrentToggleState => on;

    public UnityEvent ToggledOn;
    public UnityEvent ToggledOff;

    public override void Initialize()
    {
        base.Initialize();
        on = initiallyOn;

        if (invokeToggleOnIntialize)
        {
            InvokeToggle();
        }
    }

    protected override void OnButtonClick()
    {
        base.OnButtonClick();

        // if the button is on and cannot be turned off via clicking, do nothing
        if (supressToggleOffOnClick && on || manageToggleExternallyOnAllClicks)
        {
            return;
        }

        on = !on;
        InvokeToggle();
    }

    protected virtual void InvokeToggle()
    {
        if (on)
        {
            InvokeToggledOn();
        }
        else
        {
            InvokeToggledOff();
        }
    }

    protected virtual void InvokeToggledOn()
    {
        ToggledOn?.Invoke();
    }

    protected virtual void InvokeToggledOff()
    {
        ToggledOff?.Invoke();
    }

    public void Toggle(bool on, bool invokeButtonClick = true)
    {
        if (on != this.on)
        {
            if(invokeButtonClick)
            {
                base.OnButtonClick();
            }

            this.on = on;
            InvokeToggle();
        }
    }

    public void ForceState(bool state)
    {
        on = state;
        InvokeToggle();
    }
}
