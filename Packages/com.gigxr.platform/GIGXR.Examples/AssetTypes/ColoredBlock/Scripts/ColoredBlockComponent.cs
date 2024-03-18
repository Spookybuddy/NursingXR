namespace GIGXR.Examples.AssetTypes.ColoredBlock.Scripts
{
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using UnityEngine;

    public class ColoredBlockComponent : BaseAssetTypeComponent<ColoredBlockAssetData>
    {
        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Color";
            assetData.description.designTimeData.defaultValue = "Color of the material";

            assetData.color.designTimeData.defaultValue = Color.red;
            assetData.color.designTimeData.isEditableByAuthor = true;
        }

        [RegisterPropertyChange(nameof(ColoredBlockAssetData.color))]
        private void ChangeColor(AssetPropertyChangeEventArgs e) => ChangeColor((Color)e.AssetPropertyValue);
        public void ChangeColor(Color color)
        {
            meshRenderer.material.SetColor(ColorId, color);
        }

        protected override void Setup()
        {
            
        }

        protected override void Teardown()
        {
            
        }
    }
}