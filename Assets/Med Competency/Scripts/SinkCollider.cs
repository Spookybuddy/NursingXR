using System.Collections;
using System.Collections.Generic;
using GIGXR.Platform.Scenarios.GigAssets;
using UnityEngine;

public class SinkCollider : MonoBehaviour
{
    //The med verifier in the scene
    [SerializeField] private RoomChangeAssetData room;
    [SerializeField] private SinkAssetTypeComponent SinkAssetTypeComponent;

    private bool handsWashed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Cup")
        {

        }
        else if (!handsWashed)
        {
            SinkAssetTypeComponent.OnHandsWashed();
            handsWashed = true;
        }
    }

    [RegisterPropertyChange(nameof(RoomChangeAssetData.inPatientRoom))]
    private void resetHandWash()
    {
        handsWashed = false;
    }

}
