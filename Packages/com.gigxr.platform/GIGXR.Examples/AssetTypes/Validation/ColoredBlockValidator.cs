using GIGXR.Platform.Scenarios.GigAssets.Validation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColoredBlockValidator// : IAssetPropertyDataValidator<Color>
{
    public ValidatorEnums.ApplicationTimes ApplicationTime => ValidatorEnums.ApplicationTimes.Both;

    public bool IsValid(Color data)
    {
        return data.r >= ((data.g + data.b) / 2);
    }

    public Color ResolveInvalidValue(Color data)
    {
        return Color.red;
    }
}
