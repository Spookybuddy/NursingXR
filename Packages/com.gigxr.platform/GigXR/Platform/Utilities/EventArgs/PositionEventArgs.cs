namespace GIGXR.Platform.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Event data for holding scale data as a Vector3.
    /// </summary>
    public class PositionEventArgs : EventArgs
    {
        public PositionEventArgs(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }
}