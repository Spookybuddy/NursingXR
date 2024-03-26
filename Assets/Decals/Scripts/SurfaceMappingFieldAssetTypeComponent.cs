using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using UnityEngine;

public class SurfaceMappingFieldAssetTypeComponent : BaseAssetTypeComponent<SurfaceMappingFieldAssetData>
{
    [Tooltip("Corner transforms, listed in a Z shape.")]
    [SerializeField] private Transform[] meshCorners;

    [Tooltip("Render meshes.")]
    [SerializeField] private MeshFilter meshFilter;

    [Tooltip("Range of raycast to detect surfaces, in meters.")]
    [SerializeField] private float raycastRange;

    [Tooltip("Raycast check against only these layers. \nInclude layer of spatial awareness (31)")]
    [SerializeField] private LayerMask spatialLayer;

    //Mesh 
    private Mesh mesh;
    private Vector3[] vertices = new Vector3[5];

    //Fixed values for mesh creation
    private readonly int[] Tris = new int[] { 0, 1, 2, 0, 2, 4, 0, 4, 3, 0, 3, 1 };
    private readonly Vector2[] UVs = new Vector2[] { new(0.5f, 0.5f), Vector2.up, Vector2.one, Vector2.zero, Vector2.right };

    private IScenarioManager scenarioManager;

    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides

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

    #endregion

    #region Manipulation Functions

    public void OnManipulationStart()
    {
        //If the scenario is not editing, do nothing. Editing enum is missing?
        //if (scenarioManager.ScenarioStatus == ScenarioStatus.Playing) return;

        //Returns mesh to quad shape for easier visualization while adjusting
        for (int i = 0; i < meshCorners.Length; i++) vertices[i + 1] = new Vector3(meshCorners[i].localPosition.x, meshCorners[i].localPosition.y, 0);
        UpdateMesh();
        //assetData.decal.runtimeData.Value = mesh;
    }

    public void OnManipulationEnd()
    {
        //If the scenario is not editing, do nothing. Editing enum is missing?
        //if (scenarioManager.ScenarioStatus == ScenarioStatus.Playing) return;

        //Finds the normals of the surface and snaps to it, then finding the normals from the corners and adjusting those as well
        Debug.DrawRay(transform.position, transform.forward, Color.yellow, 10, false);
        Debug.LogWarning(transform.name);
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastRange, spatialLayer)) {
            transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
            Debug.DrawRay(transform.position, transform.forward, Color.green, 10, false);
            for (int i = 0; i < meshCorners.Length; i++) {
                //Ray pointing towards center and backwards
                Vector3 inward = transform.forward - (meshCorners[i].localPosition.x * transform.right) - (meshCorners[i].localPosition.y * transform.up);

                //If surface is not found resort to original position
                if (Physics.Raycast(meshCorners[i].position, inward, out RaycastHit surface, raycastRange, spatialLayer)) vertices[i + 1] = transform.InverseTransformPoint(surface.point);
                else vertices[i + 1] = new Vector3(meshCorners[i].localPosition.x, meshCorners[i].localPosition.y, 0);
            }

            //Update the data
            UpdateMesh();
            //assetData.decal.runtimeData.Value = mesh;
        }
    }

    #endregion
    /*
    #region Property Change Handlers

    [RegisterPropertyChange(nameof(SurfaceMappingFieldAssetData.decal))]
    private void OnDecalChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized) return;

        Mesh newValue = (Mesh)args.AssetPropertyValue;

        //Directly update mesh filter and collider (to avoid feedback loops I think)
        meshFilter.mesh = newValue;
        Debug.Log("Networked mesh update");
        if (TryGetComponent<MeshCollider>(out MeshCollider collider)) collider.sharedMesh = newValue;
    }

    #endregion
    */
    //Update mesh with new data
    private void UpdateMesh()
    {
        mesh.Clear();
        meshFilter.mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = UVs;
        mesh.triangles = Tris;
        mesh.RecalculateNormals();

        //If a mesh collider property exists assign the mesh to it
        if (TryGetComponent<MeshCollider>(out MeshCollider collider)) collider.sharedMesh = mesh;
    }
}