namespace GIGXR.Platform.HMD.UI
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "New Toolbar Button", menuName = "GIGXR/ScriptableObjects/ToolbarButtonScriptableObject")]
    public class ToolbarButtonScriptableObject : ScriptableObject
    {
        public bool isButtonEnabled;

        public bool onlyVisibleInsideSession;

        public bool isHostOnly;

        public string speechCommand;

        public int orderInToolbar;

        public LabeledIconScriptableObject iconInformation;

        // Optional, if set will remove any toolbar buttons associated with this screen
        public BaseScreenObject.ScreenType ScreenTypeOverride;
    }
}