using GIGXR.Examples.AssetTypes.ColoredSphere.Scripts;
using GIGXR.Examples.Components.ColorChange;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using UnityEngine;
using Random = UnityEngine.Random;

public class DemoSphereComponent : BaseAssetTypeComponent<DemoSphereAssetData>, IDemoSphereComponent
{
    private IColorChangeComponent colorChangeComponent;

    private void Awake()
    {
        colorChangeComponent = GetComponent<IColorChangeComponent>();
    }

    private void Update()
    {
        // Push 9 on the keyboard for a random color
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            assetData.validatedVector.runtimeData.Value = new Vector3(assetData.validatedVector.runtimeData.Value.x, Random.Range(1.5f, 2.5f), assetData.validatedVector.runtimeData.Value.z);
        }
    }

    public override void SetEditorValues()
    {
        assetData.name.designTimeData.defaultValue = "Sphere";
        assetData.description.designTimeData.defaultValue = "Data needed for a sphere.";
        assetData.color.designTimeData.defaultValue = Color.green;
        assetData.randomIntToDemoConditions.designTimeData.defaultValue = 1;
    }

    public void ChangeRandomColor()
    {
        assetData.color.runtimeData.Value = Random.ColorHSV();
    }

    protected override void Setup()
    {
        
    }

    protected override void Teardown()
    {
        
    }

    [RegisterPropertyChange(nameof(DemoSphereAssetData.color))]
    private void OnColorChanged(AssetPropertyChangeEventArgs e) => OnColorChanged((Color)e.AssetPropertyValue);
    private void OnColorChanged(Color color)
    {
        colorChangeComponent.ChangeColor(color);
    }
}