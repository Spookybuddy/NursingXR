using UnityEditor;
using UnityEngine;
using PropertyDrawer = UnityEditor.PropertyDrawer;

namespace GIGXR.Platform.Utilities
{
    [CustomPropertyDrawer(typeof(ShowInPlayerModeAttribute))]
    public class ShowInPlayerModeDrawer : PropertyDrawer
    {
        public override void OnGUI
        (
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            // bool wasEnabled = GUI.enabled;

            bool enabled = GetConditionalShowAttributeResult
                ((ShowInPlayerModeAttribute)attribute);

            if (enabled)
            {
                EditorGUI.PropertyField
                    (position,
                     property,
                     label,
                     true);
            }

            // GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool enabled = GetConditionalShowAttributeResult
                ((ShowInPlayerModeAttribute)attribute);

            if (enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalShowAttributeResult
            (ShowInPlayerModeAttribute attributeData)
        {
            switch (Application.isPlaying)
            {
                case true when (attributeData.ShowInThisMode == UnityPlayerModes.PlayMode):
                case false when (attributeData.ShowInThisMode == UnityPlayerModes.EditMode):
                    return true;
                default:
                    return false;
            }
        }
    }
}