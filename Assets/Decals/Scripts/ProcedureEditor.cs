using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProcedureFieldAssetTypeComponent), true)]
[CanEditMultipleObjects]
public class ProcedureEditor : Editor
{
    private int _arraySize;
    private GameObject[] _medicalSupplies;
    private ProcedureFieldAssetTypeComponent _manager;
    private int _usedOnStep;
    private int _usedBy;
    private string[] _methods = new string[] { "Nothing", "Touch", "Spray", "Ointment", "Stay", "Exit" };
    private bool _isInUse;
    private bool _onCorrectStep;

    private bool foldout;

    private readonly GUIContent[] _displays = new GUIContent[] {
        new GUIContent("Manager Object", "The manager object that networks the steps."),
        new GUIContent("Used On Step", "Which step the object is to be used on."),
        new GUIContent("Object Use", "How the object is to be used."),
        new GUIContent("Is In Use", "The object is currently being manipulated."),
        new GUIContent("On Correct Step", "Used On Step matches the manager's step."),
        new GUIContent("Number of Medical Supplies", "Length of the array containing app medical supplies."),
        new GUIContent("List Of Medical Supplies", "Array containing prefabs."),
        new GUIContent("Medical Supply", "Medical Supply Prefab in scene.")
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ProcedureFieldAssetTypeComponent script = target as ProcedureFieldAssetTypeComponent;

        //Show specific variables depending on manager status
        if (script.isManager) {
            //Check values first
            if (script.medicalSupplies != null) _medicalSupplies = script.medicalSupplies;
            if (script.arraySize > 0) _arraySize = script.arraySize;

            //List of objects
            EditorGUI.BeginChangeCheck();
            _arraySize = EditorGUILayout.IntField(_displays[5], _arraySize);
            if (EditorGUI.EndChangeCheck()) {
                //Minimum 0
                if (_arraySize <= 0) {
                    script.arraySize = 0;
                    script.medicalSupplies = new GameObject[0];
                    return;
                }

                script.arraySize = _arraySize;
                _medicalSupplies = new GameObject[_arraySize];

                //Copy any variables already set
                if (script.medicalSupplies.Length != _arraySize) {
                    for (int i = 0; i < Mathf.Min(script.medicalSupplies.Length, _arraySize); i++) {
                        if (script.medicalSupplies[i] != null) _medicalSupplies[i] = script.medicalSupplies[i];
                    }
                    script.medicalSupplies = _medicalSupplies;
                }
            }

            //Creates X number of fields for the prefabs
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, _displays[6]);
            if (_arraySize > 0 && foldout) {
                for (int i = 0; i < _arraySize; i++) {
                    //GameObjects
                    EditorGUI.BeginChangeCheck();
                    _medicalSupplies[i] = EditorGUILayout.ObjectField(_displays[7], _medicalSupplies[i], typeof(GameObject), false) as GameObject;
                    if (EditorGUI.EndChangeCheck()) script.medicalSupplies[i] = _medicalSupplies[i];
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        } else {
            //Check for values first
            if (script.manager != null) _manager = script.manager;
            if (script.usedOnStep != 0) _usedOnStep = script.usedOnStep;
            if (script.usedBy != 0) _usedBy = script.usedBy;
            if (script.isInUse) _isInUse = true;
            if (script.onCorrectStep) _onCorrectStep = true;

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
        }
    }
}