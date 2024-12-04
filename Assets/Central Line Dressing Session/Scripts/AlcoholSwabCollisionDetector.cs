using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlcoholSwabCollisionDetector : MonoBehaviour
{
    private bool swabNotTriggered = true;
    [SerializeField] private AlcoholSwabAssetTypeComponent swabParent;
    
    private void OnTriggerEnter(Collider other)
    {
        if (swabNotTriggered && other.name.Contains("Chlorhexadine Clean Hitbox "))
        {
            swabParent.SwabHitBoxTriggered();
            swabNotTriggered = false;
        }
    }
}
