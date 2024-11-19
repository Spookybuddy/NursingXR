using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HandDetectorCollisionDetector : MonoBehaviour
{
    [SerializeField] private string jointName;
    private bool noChlorhexadine = true;
    private ChlorhexadineAssetTypeComponent chlorhexadine;
    
    #region Collision & Trigger Functions

    private void OnTriggerEnter(Collider other)
    {
        if (noChlorhexadine)
        {
            chlorhexadine = transform.parent.parent.gameObject.GetComponent<HandDetectorAssetTypeComponent>().GetChlorhexadine();
            if (chlorhexadine != null)
            {
                noChlorhexadine = false;
            }
        }
        if (other.CompareTag("FindObjectName"))
        {
            string objName = other.name;
            if (objName == "Left Wing Hitbox 1" ||
                objName == "Left Wing Hitbox 2" ||
                objName == "Right Wing Hitbox 1" ||
                objName == "Right Wing Hitbox 2")
            {
                chlorhexadine.WingHitBoxTriggered(jointName, objName);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (noChlorhexadine)
        {
            chlorhexadine = transform.parent.parent.gameObject.GetComponent<HandDetectorAssetTypeComponent>().GetChlorhexadine();
            Debug.Log(transform.parent.parent.gameObject.name);
            if (chlorhexadine != null)
            {
                noChlorhexadine = false;
            }
        }
        if (other.CompareTag("FindObjectName"))
        {
            string objName = other.name;
            if (objName == "Left Wing Hitbox 1" ||
                objName == "Left Wing Hitbox 2" ||
                objName == "Right Wing Hitbox 1" ||
                objName == "Right Wing Hitbox 2")
            {
                chlorhexadine.WingHitBoxExited(jointName, objName);
            }
        }
    }

    #endregion
}
