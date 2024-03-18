using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

namespace GIGXR.Platform.UI.HMD.UiConfig
{
    [RequireComponent(typeof(Microsoft.MixedReality.Toolkit.UI.Interactable))]
    [RequireComponent(typeof(NearInteractionTouchable))]
    [RequireComponent(typeof(PhysicalPressEventRouter))]
    [RequireComponent(typeof(Microsoft.MixedReality.Toolkit.UI.PressableButtonHoloLens2))]
    public class ButtonUiConfig : BaseUiObject
    {
        [Header("Button permissions")]
        [SerializeField] private bool RestrictToHostUser;
        
        [Header("Button look and feel")]
        [SerializeField] private bool IsSelectable;

        private GameObject highlightObject;
        private GameObject nonHighlightObject; 

        public void Awake()
        {
            // appEventBus.Subscribe<>();

            // if (IsSelectable)
            // {
            //     nonHighlightObject = transform.Find("Quad").gameObject;
            //     highlightObject = transform.Find("Highlight Quad").gameObject;
            // }
        }

        public void SetHighlight(bool isHighlighted)
        {
            if (nonHighlightObject == null || highlightObject == null)
            {
                return; 
            }
            
            nonHighlightObject.SetActive(!isHighlighted);
            highlightObject.SetActive(isHighlighted);
        }

        protected override void SubscribeToEventBuses()
        { 
            // Not needed in this class
        }
    }
}
