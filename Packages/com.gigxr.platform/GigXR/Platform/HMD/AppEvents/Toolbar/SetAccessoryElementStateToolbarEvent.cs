namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;
    using UnityEngine;

    public class SetAccessoryElementStateToolbarEvent : BaseToolbarEvent
    {
        public bool AccessoryState;

        public SetAccessoryElementStateToolbarEvent(bool state) : base()
        {
            AccessoryState = state;
        }
    }
}