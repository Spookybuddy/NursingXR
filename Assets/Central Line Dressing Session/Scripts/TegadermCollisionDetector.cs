using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TegadermCollisionDetector : MonoBehaviour
{
    [SerializeField] private TegadermAssetTypeComponent tegaderm;
    private bool notCompletedStep6 = true;

    public void CompletedStep6()
    {
        notCompletedStep6 = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FindObjectName") && other.name == "Tegaderm Placement Hitbox" && notCompletedStep6)
        {
            tegaderm.CheckStep6(other);
        }
    }
}
