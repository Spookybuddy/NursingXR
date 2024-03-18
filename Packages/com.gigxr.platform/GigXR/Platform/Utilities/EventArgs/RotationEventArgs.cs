namespace GIGXR.Platform.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Event data for holding rotation data as a Quaternion.
    /// </summary>
    public class RotationEventArgs : EventArgs
    {
        public RotationEventArgs(Quaternion rotation)
        {
            Rotation = rotation;
        }

        public Quaternion Rotation { get; }
    }
}