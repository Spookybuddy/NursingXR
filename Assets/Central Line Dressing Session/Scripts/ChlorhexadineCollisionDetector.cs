using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChlorhexadineCollisionDetector : MonoBehaviour
{
    [SerializeField] private ChlorhexadineAssetTypeComponent chlorhexadine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FindObjectName"))
        {
            chlorhexadine.TestForStep3Progress(other);
        }
    }
}
