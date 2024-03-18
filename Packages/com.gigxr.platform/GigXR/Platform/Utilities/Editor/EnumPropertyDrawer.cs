using System;
using UnityEditor;
using UnityEngine;
using PropertyDrawer = UnityEditor.PropertyDrawer;

namespace GIGXR.Platform.Utilities
{
    [CustomPropertyDrawer(typeof(ShowBasedOnEnumAttribute))]
    public class EnumPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI
        (
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            bool enabled = GetConditionalShowAttributeResult((ShowBasedOnEnumAttribute)attribute, property);

            if (enabled)
            {
                EditorGUI.PropertyField
                    (position,
                     property,
                     label,
                     true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool enabled = GetConditionalShowAttributeResult
                ((ShowBasedOnEnumAttribute)attribute, property);

            if (enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalShowAttributeResult(ShowBasedOnEnumAttribute attributeData, SerializedProperty property)
        {
            var allNames = Enum.GetNames(attributeData.enumType);
            var attributeIndex = Array.IndexOf(allNames, attributeData.enumValue);
            var enumProperty = property.serializedObject.FindProperty(attributeData.enumPropertyValueName);

            return attributeIndex == enumProperty.enumValueIndex;
        }
    }
}