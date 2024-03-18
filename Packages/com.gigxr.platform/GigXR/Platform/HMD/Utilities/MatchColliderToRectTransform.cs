namespace GIGXR.Platform.HMD.Utilities
{
    using Microsoft.MixedReality.Toolkit.Input;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Utility script for UI components to make sure that a box collider matches the size of the RectTransform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(NearInteractionTouchable))]
    public class MatchColliderToRectTransform : MonoBehaviour
    {
        [SerializeField]
        private float customButtonZSize = 0;

        private BoxCollider attachedCollider;
        private RectTransform rectTransform;
        private NearInteractionTouchable nearInteractionTouchable;

        private float defaultZSize;
        private float zSizeToUse;

        void Start()
        {
            // This component will only work on components where the scale is 1 and uniform
            if (transform.localScale != Vector3.one)
            {
                Destroy(this);
            }
            else
            {
                attachedCollider = GetComponent<BoxCollider>();
                rectTransform = GetComponent<RectTransform>();
                nearInteractionTouchable = GetComponent<NearInteractionTouchable>();

                defaultZSize = attachedCollider.size.z;
            }
        }     

        void Update()
        {
            zSizeToUse = customButtonZSize != 0 ? customButtonZSize : defaultZSize;

            if (attachedCollider.size.x != rectTransform.sizeDelta.x || 
               attachedCollider.size.y != rectTransform.sizeDelta.y || 
               attachedCollider.size.z != zSizeToUse)
            {
                attachedCollider.size = new Vector3(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y, zSizeToUse);

                // Reset the near interactable collider to update the center and size
                nearInteractionTouchable.SetTouchableCollider(attachedCollider);
            }

        }
    }
}