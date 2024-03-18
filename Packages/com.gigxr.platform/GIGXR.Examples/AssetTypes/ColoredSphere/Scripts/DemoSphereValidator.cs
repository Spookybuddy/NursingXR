using GIGXR.Platform.Scenarios.GigAssets.Validation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSphereValidator// : IAssetPropertyDataValidator<Vector3>
{
    public ValidatorEnums.ApplicationTimes ApplicationTime => ValidatorEnums.ApplicationTimes.Both;

    public bool IsValid(Vector3 data)
    {
        return data.y <= 2.0f;
    }

    public Vector3 ResolveInvalidValue(Vector3 data)
    {
        return new Vector3(data.x, 2.0f, data.z);
    }
}
