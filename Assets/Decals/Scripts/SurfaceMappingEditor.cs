using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SurfaceMappingFieldAssetTypeComponent), true)]
[CanEditMultipleObjects]
public class SurfaceMappingEditor : Editor
{
    private Transform[] _meshCorners = new Transform[4];
    private Transform[] _meshHoles = new Transform[8];
    private float _holeDepth;

    private bool showHoles;
    private bool showCorners;

    private readonly GUIContent[] _displays = new GUIContent[] {
        new GUIContent("Vertex Count", "Number of vertices that form the hole."),
        new GUIContent("Transforms", "The transforms of the hole, in a clockwise order."),
        new GUIContent("Transforms", "The transforms of the corners of the mesh, in a Z shape."),
        new GUIContent("Hole Depth", "The depth of the indent."),
        new GUIContent("Vertex Position", "The positions of vertices.")
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SurfaceMappingFieldAssetTypeComponent script = target as SurfaceMappingFieldAssetTypeComponent;

        //Show different variables based on type of decal
        if (script.indentation) {
            //Check if values exist
            if (script.holeDepth != 0) _holeDepth = script.holeDepth;
            if (script.meshHoles.Length > 0) _meshHoles = script.meshHoles;
            else script.meshHoles = new Transform[_meshHoles.Length];

            //Depth of the hole
            EditorGUI.BeginChangeCheck();
            _holeDepth = EditorGUILayout.FloatField(_displays[3], _holeDepth);
            if (EditorGUI.EndChangeCheck()) script.holeDepth = _holeDepth;

            //List of hole transforms recorded
            showHoles = EditorGUILayout.BeginFoldoutHeaderGroup(showHoles, _displays[1]);
            if (showHoles) {
                for (int i = 0; i < script.meshHoles.Length; i++) {
                    EditorGUI.BeginChangeCheck();
                    _meshHoles[i] = EditorGUILayout.ObjectField(_displays[4], _meshHoles[i], typeof(Transform), false) as Transform;
                    if (EditorGUI.EndChangeCheck()) script.meshHoles[i] = _meshHoles[i];
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

        } else {
            //Check if values exist
            if (script.meshCorners.Length > 0) _meshCorners = script.meshCorners;
            else script.meshCorners = new Transform[_meshCorners.Length];

            //List of corner transforms recorded
            showCorners = EditorGUILayout.BeginFoldoutHeaderGroup(showCorners, _displays[2]);
            if (showCorners) {
                for (int i = 0; i < script.meshCorners.Length; i++) {
                    EditorGUI.BeginChangeCheck();
                    _meshCorners[i] = EditorGUILayout.ObjectField(_displays[4], _meshCorners[i], typeof(Transform), false) as Transform;
                    if (EditorGUI.EndChangeCheck()) script.meshCorners[i] = _meshCorners[i];
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}