namespace GIGXR.Platform.HMD.UI.Layouts
{
    using Microsoft.MixedReality.Toolkit.UI;
    using UnityEngine;

    public class ScrollButtonLayout : MonoBehaviour
    {
        [SerializeField]
        private Interactable scrollUpButton; 
        
        [SerializeField]
        private Interactable scrollDownButton;
        
        public Interactable ScrollUpButton
        {
            get => scrollUpButton;
            set => scrollUpButton = value;
        }
        
        public Interactable ScrollDownButton
        {
            get => scrollDownButton;
            set => scrollDownButton = value;
        }
    }
}
