using System;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using UnityEngine;

[Serializable]
public class RoomChangeAssetData : BaseAssetData
{
    //Determines whther the user is in the med room or patient room
    public AssetPropertyDefinition<bool> inPatientRoom;
}
