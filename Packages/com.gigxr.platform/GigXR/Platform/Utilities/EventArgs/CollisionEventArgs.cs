namespace GIGXR.Platform.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Event data for holding Collision from physic interactions.
    /// </summary>
    public class CollisionEventArgs : EventArgs
    {
        public CollisionEventArgs(Collision collision)
        {
            Collision = collision;
        }

        public Collision Collision { get; }
    }
}