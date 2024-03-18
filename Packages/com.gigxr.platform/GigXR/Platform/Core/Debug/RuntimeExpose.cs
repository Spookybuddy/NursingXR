using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an attribute to help the debbuging of some variables inside runtime by using a hidden debug panel with the user Instruments Hand
/// Usage: add the attribute [RuntimeExpose("Optional Display Name")] to any field inside a monobehaviour.
/// Then, open the Instrument hands menu and click in the small debug icon in the lower part to see your attribute getting tracked in runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RuntimeExpose : Attribute
{
    public string DisplayName;
    
    public RuntimeExpose(string displayName)
    {

        DisplayName = displayName;
    }

    public RuntimeExpose()
    {
        DisplayName = "";
    }

}
