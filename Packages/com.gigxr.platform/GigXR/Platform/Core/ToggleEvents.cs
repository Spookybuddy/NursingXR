using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleEvents : MonoBehaviour
{
    public UnityEvent ToggleOn;

    public UnityEvent ToggleOff;

    public void SetToggleState(bool state)
    {
        if(state)
        {
            ToggleOn?.Invoke();
        }
        else
        {
            ToggleOff?.Invoke();
        }
    }
}
