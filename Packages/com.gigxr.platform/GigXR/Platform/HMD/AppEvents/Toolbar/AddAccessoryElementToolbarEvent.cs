namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;
    using UnityEngine;

    public class AddAccessoryElementToolbarEvent : BaseToolbarEvent
    {
        public BaseScreenObject.ScreenType ToolbarScreenTypeButton;
        public GameObject AccessoryElement;

        public AddAccessoryElementToolbarEvent(BaseScreenObject.ScreenType screenType, GameObject accessoryElement) : base()
        {
            ToolbarScreenTypeButton = screenType;
            AccessoryElement = accessoryElement;
        }
    }
}