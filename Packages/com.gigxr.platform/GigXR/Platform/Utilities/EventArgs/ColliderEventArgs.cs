namespace GIGXR.Platform.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Event data for holding Collider data from a trigger collision.
    /// </summary>
    public class ColliderEventArgs : EventArgs
    {
        public ColliderEventArgs(Collider collider)
        {
            Collider = collider;
        }

        public Collider Collider { get; }
    }
}