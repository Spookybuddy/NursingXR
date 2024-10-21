using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutopsyBodyPartTriggerDetector : MonoBehaviour
{
    [SerializeField]
    private AutopsyBodyPartAssetTypeComponent parentComponent;

    #region Collision and Trigger Functions

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GravityZone"))
        {
            parentComponent.EnterGravityZone(other);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("GravityZone"))
        {
            parentComponent.ExitGravityZone(other);
        }
    }

    #endregion
}
