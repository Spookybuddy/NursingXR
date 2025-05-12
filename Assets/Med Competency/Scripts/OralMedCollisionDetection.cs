using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OralMedCollisionDetection : MonoBehaviour
{
    //The med verifier in the scene
    [SerializeField] private MedVerifier medVerifier;
    
    private bool medEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if ( !medEntered )
        {
            medVerifier.OralMedCheck();

            medEntered = true;
        }
    }
}

