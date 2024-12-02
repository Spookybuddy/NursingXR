using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GauzeCollisionDetector : MonoBehaviour
{
    [SerializeField] private GauzeAssetTypeComponent gauze;
    private bool notCheckedStep4 = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FindObjectName") && other.name.Contains("Chlorhexadine Clean Hitbox ") && notCheckedStep4)
        {
            gauze.CheckStep4();
            notCheckedStep4 = false;
        }
    }
}
