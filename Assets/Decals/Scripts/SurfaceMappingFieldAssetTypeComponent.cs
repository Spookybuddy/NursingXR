using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using UnityEngine;

public class SurfaceMappingFieldAssetTypeComponent : BaseAssetTypeComponent<SurfaceMappingFieldAssetData>
{
    [Header("Mesh overlay that will warp to scanned surface")]

    [Tooltip("Center of manipulated object.")]
    [SerializeField] private Transform center;

    [Tooltip("Corner transforms, listed in a Z shape.")]
    [SerializeField] private Transform[] meshCorners;

    [Tooltip("Render meshes.")]
    [SerializeField] private MeshFilter meshFilter;

    [Tooltip("Range of raycast to detect surfaces, in meters.")]
    [SerializeField] private float raycastRange;

    [Tooltip("Raycast check against only these layers. \nInclude layer of spatial awareness (31)")]
    [SerializeField] private LayerMask spatialLayer;

    [Header("Higher detailed mesh variables")]

    [Tooltip("Should the mesh physically have a hole?")]
    [SerializeField] private bool indentation;

    [Tooltip("If indentation is true, the transforms of the hole, in a clockwise order.")]
    [SerializeField] private Transform[] meshHoles;

    [Tooltip("If indentation is true, the depth of the indent.")]
    [SerializeField] private float holeDepth;

    //Mesh 
    private Mesh mesh;
    private Vector3[] vertices = new Vector3[5];

    //Fixed values for mesh creation
    private readonly int[] Tris = new int[] {
        0, 1, 2,
        0, 2, 4,
        0, 4, 3,
        0, 3, 1
    };
    private readonly Vector2[] UVs = new Vector2[] {
        new(0.5f, 0.5f),
        Vector2.up,
        Vector2.one,
        Vector2.zero,
        Vector2.right
    };

    //Fixed values for cavity creation
    private Vector3[] Cav_vertices = new Vector3[17];
    private readonly int[] Cav_Tris = new int[] {
        0, 1, 2,
        0, 2, 3,
        0, 3, 4,
        0, 4, 5,
        0, 5, 6,
        0, 6, 7,
        0, 7, 8,
        0, 8, 1,
        10, 2, 1,
        1, 9, 10,
        11, 3, 2,
        2, 10, 11,
        12, 4, 3,
        3, 11, 12,
        13, 5, 4,
        4, 12, 13,
        14, 6, 5,
        5, 13, 14,
        15, 7, 6,
        6, 14, 15,
        16, 8, 7,
        7, 15, 16,
        9, 1, 8,
        8, 16, 9
    };
    private readonly Vector2[] Cav_UVs = new Vector2[17];

    private IScenarioManager scenarioManager;

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    public override void SetEditorValues()
    {

    }

    protected override void Setup()
    {
        mesh = new Mesh();
    }

    protected override void Teardown()
    {
        mesh.Clear();
    }

    public void OnManipulationStart()
    {
        //If the scenario is not editing, do nothing. Editing enum is missing?
        if (scenarioManager.ScenarioStatus == ScenarioStatus.Playing) return;

        //Returns mesh to quad shape for easier visualization while adjusting
        for (int i = 0; i < meshCorners.Length; i++) vertices[i + 1] = new Vector3(meshCorners[i].localPosition.x, meshCorners[i].localPosition.y, 0);
        UpdateOverlay();

        assetData.updateMesh.runtimeData.Value -= 1;
    }

    public void OnManipulationEnd()
    {
        //If the scenario is not editing, do nothing. Editing enum is missing?
        //if (scenarioManager.ScenarioStatus == ScenarioStatus.Playing) return;

        //Finds the normals of the surface and snaps to it, then finding the normals from the corners and adjusting those as well
        if (Physics.Raycast(center.position, center.forward, out RaycastHit hit, raycastRange, spatialLayer)) {
            center.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));

            //Generate overlay or cavity
            if (indentation) {
                GenerateCavity();
                UpdateCavity();
            } else {
                GenerateOverlay();
                UpdateOverlay();
            }

            assetData.updateMesh.runtimeData.Value += 1;
        }
    }

    //Warp the corners to the surface
    private void GenerateOverlay()
    {
        for (int i = 0; i < meshCorners.Length; i++) {
            //Ray pointing towards center and backwards
            Vector3 inward = center.forward - (meshCorners[i].localPosition.x * center.right) - (meshCorners[i].localPosition.y * center.up);
            Debug.DrawRay(meshCorners[i].position, inward * raycastRange, Color.cyan, 5);

            //If surface is not found resort to original position
            if (Physics.Raycast(meshCorners[i].position, inward, out RaycastHit surface, raycastRange, spatialLayer)) vertices[i + 1] = center.InverseTransformPoint(surface.point);
            else vertices[i + 1] = new Vector3(meshCorners[i].localPosition.x, meshCorners[i].localPosition.y, 0);
        }
    }

    //Generate the hole, and warp to surface
    private void GenerateCavity()
    {
        //Center in hole
        Vector3 middle = new Vector3(0, 0, holeDepth);
        Cav_vertices[0] = 1.5f * middle;
        Cav_UVs[0] = new Vector2(0.5f, 0.5f);

        //Update vertex and uv positions based off of transform list
        for (int i = 0; i < meshHoles.Length; i++)
        {
            //Positions
            Cav_vertices[i + 1] = meshHoles[i].localPosition + middle;
            Cav_vertices[i + meshHoles.Length + 1] = meshHoles[i].localPosition;

            //Raycast to surface
            Vector3 inward = center.forward - (meshHoles[i].localPosition.x * center.right) - (meshHoles[i].localPosition.y * center.up);
            Debug.DrawRay(meshHoles[i].position, inward * raycastRange, Color.cyan, 5);

            if (Physics.Raycast(meshHoles[i].position, inward, out RaycastHit surface, raycastRange, spatialLayer)) {
                Cav_vertices[i + 1] = center.InverseTransformPoint(surface.point) + middle;
                Cav_vertices[i + meshHoles.Length + 1] = center.InverseTransformPoint(surface.point);
            }

            //UVs
            Cav_UVs[i + 1] = new Vector2(0.5f + meshHoles[i].localPosition.x, 0.5f + meshHoles[i].localPosition.y);
            float x = Mathf.Sign(meshHoles[i].localPosition.x) * holeDepth;
            float y = Mathf.Sign(meshHoles[i].localPosition.y) * holeDepth;
            Cav_UVs[i + meshHoles.Length + 1] = new Vector2(0.5f + x + meshHoles[i].localPosition.x, 0.5f + y + meshHoles[i].localPosition.y);
        }
    }

    //Detect changes in the networked variable, and update the mesh client-side (Position should be already networked)
    [RegisterPropertyChange(nameof(SurfaceMappingFieldAssetData.updateMesh))]
    private void OnUpdateMeshChanged(AssetPropertyChangeEventArgs args)
    {
        Debug.LogWarning("Value has changed");

        if (!IsInitialized) return;

        Mesh newValue = (Mesh)args.AssetPropertyValue;

        //Directly update mesh filter and collider (to avoid feedback loops I think)
        meshFilter.mesh = newValue;
        Debug.Log("Networked mesh update");
        if (TryGetComponent<MeshCollider>(out MeshCollider collider)) collider.sharedMesh = newValue;
    }

    //Update mesh
    private void UpdateOverlay()
    {
        mesh.Clear();
        meshFilter.mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = UVs;
        mesh.triangles = Tris;
        mesh.RecalculateNormals();

        //If a mesh collider property exists assign the mesh to it
        if (TryGetComponent(out MeshCollider collider)) collider.sharedMesh = mesh;
    }

    //Update mesh with hole
    private void UpdateCavity()
    {
        mesh.Clear();
        meshFilter.mesh = mesh;
        mesh.vertices = Cav_vertices;
        mesh.uv = Cav_UVs;
        mesh.triangles = Cav_Tris;
        mesh.RecalculateNormals();

        //If a mesh collider property exists assign the mesh to it
        if (TryGetComponent(out MeshCollider collider)) collider.sharedMesh = mesh;
    }
}