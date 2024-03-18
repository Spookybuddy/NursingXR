namespace GIGXR.Platform.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Event data for holding position data as a Vector3.
    /// </summary>
    public class ScaleEventArgs : EventArgs
    {
        public ScaleEventArgs(Vector3 scale)
        {
            Scale = scale;
        }

        public Vector3 Scale { get; }
    }
}