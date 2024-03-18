using UnityEngine;

namespace GIGXR.Examples.Components.ColorChange
{
    /// <summary>
    /// A component that enables changing the color of an object with a basic URP material.
    /// </summary>
    [DisallowMultipleComponent]
    public class ColorChangeComponent : MonoBehaviour, IColorChangeComponent
    {
        private static readonly int MainColorId = Shader.PropertyToID("_BaseColor");
        private MeshRenderer meshRenderer;

        public Color CurrentColor { get; private set; }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void ChangeColor(Color color)
        {
            meshRenderer.material.SetColor(MainColorId, color);
            CurrentColor = color;
        }
    }
}