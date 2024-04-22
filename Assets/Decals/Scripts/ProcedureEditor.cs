using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProcedureFieldAssetTypeComponent), true)]
[CanEditMultipleObjects]
public class ProcedureEditor : Editor
{
    private ProcedureFieldAssetTypeComponent _manager;
    private int _usedOnStep;
    private int _usedBy;
    private string[] _methods = new string[] { "Nothing", "Touch", "Spray", "Ointment", "Stay", "Exit" };
    private bool _isInUse;
    private bool _onCorrectStep;
    private Vector3 _raycastScale;
    private float _raycastDistance;

    private readonly GUIContent[] _displays = new GUIContent[] {
        new GUIContent("Manager Object", "The manager object that networks the steps."),
        new GUIContent("Used On Step", "Which step the object is to be used on."),
        new GUIContent("Object Use", "How the object is to be used."),
        new GUIContent("Is In Use", "The object is currently being manipulated."),
        new GUIContent("On Correct Step", "Used On Step matches the manager's step."),
        new GUIContent("Boxcast Scale", "The Vector3 representing the scale of the boxcast."),
        new GUIContent("Raycast Range", "The distance the ray will travel.")
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ProcedureFieldAssetTypeComponent script = target as ProcedureFieldAssetTypeComponent;

        //Show specific variables depending on manager status
        if (!script.isManager) {
            //Check for values first
            if (script.manager != null) _manager = script.manager;
            if (script.usedOnStep != 0) _usedOnStep = script.usedOnStep;
            if (script.usedBy != 0) _usedBy = script.usedBy;
            if (script.isInUse) _isInUse = true;
            if (script.onCorrectStep) _onCorrectStep = true;
            if (script.raycastScale != Vector3.zero) _raycastScale = script.raycastScale;
            if (script.raycastDistance != 0) _raycastDistance = script.raycastDistance;

            //Manager object
            EditorGUI.BeginChangeCheck();
            _manager = EditorGUILayout.ObjectField(_displays[0], _manager, typeof(ProcedureFieldAssetTypeComponent), false) as ProcedureFieldAssetTypeComponent;
            if (EditorGUI.EndChangeCheck()) script.manager = _manager;

            //Step #
            EditorGUI.BeginChangeCheck();
            _usedOnStep = EditorGUILayout.IntField(_displays[1], _usedOnStep);
            if (EditorGUI.EndChangeCheck()) script.usedOnStep = _usedOnStep;

            //Dropdown listing of use methods
            EditorGUI.BeginChangeCheck();
            _usedBy = EditorGUILayout.Popup(_displays[2], _usedBy, _methods);
            if (EditorGUI.EndChangeCheck()) script.usedBy = _usedBy;

            //In use
            EditorGUI.BeginChangeCheck();
            _isInUse = EditorGUILayout.Toggle(_displays[3], _isInUse);
            if (EditorGUI.EndChangeCheck()) script.isInUse = _isInUse;

            //On right step
            EditorGUI.BeginChangeCheck();
            _onCorrectStep = EditorGUILayout.Toggle(_displays[4], _onCorrectStep);
            if (EditorGUI.EndChangeCheck()) script.onCorrectStep = _onCorrectStep;

            //Boxcast scale
            EditorGUI.BeginChangeCheck();
            _raycastScale = EditorGUILayout.Vector3Field(_displays[5], _raycastScale);
            if (EditorGUI.EndChangeCheck()) script.raycastScale = _raycastScale;

            //Raycast range
            EditorGUI.BeginChangeCheck();
            _raycastDistance = EditorGUILayout.FloatField(_displays[6], _raycastDistance);
            if (EditorGUI.EndChangeCheck()) script.raycastDistance = _raycastDistance;
        }
    }
}