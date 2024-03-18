namespace GIGXR.Examples.Components.ColorChange
{
    using UnityEngine;

    public interface IColorChangeComponent
    {
        public Color CurrentColor { get; }
        public void ChangeColor(Color color);
    }
}