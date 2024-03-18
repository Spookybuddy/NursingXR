using GIGXR.Platform;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomPropertyDrawer(typeof(DontShowInPrefabModeAttribute))]
public class DontShowInPrefabModeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var currentStage = PrefabStageUtility.GetCurrentPrefabStage();
        var component = (Component)property.serializedObject.targetObject;
        bool enabled = currentStage != null ? currentStage.IsPartOfPrefabContents(component.gameObject) : true;
        enabled &= Application.isPlaying;

        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var currentStage = PrefabStageUtility.GetCurrentPrefabStage();
        var component = (Component)property.serializedObject.targetObject;

        bool enabled = currentStage != null ? currentStage.IsPartOfPrefabContents(component.gameObject) : true;
        enabled &= Application.isPlaying;

        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}