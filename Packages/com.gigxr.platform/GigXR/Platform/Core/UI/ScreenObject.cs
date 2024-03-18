using UnityEngine;
using GIGXR.Platform.Interfaces;
using System;

// TODO probably a good idea to dissolve this class and just use UiObject instead.
namespace GIGXR.Platform.UI
{
    /// <summary>
    /// A generic screen used by both Mobile and HMD.
    /// </summary>
    public class ScreenObject : UiObject
    {
        public event EventHandler ScreenBroughtUp;
        public event EventHandler ScreenBroughtDown;

        private Transform startParent;
        private Vector3 startPosition;

        protected virtual void Start()
        {
            startParent = GetTransform().parent;
            startPosition = transform.localPosition;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ScreenBroughtUp?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDisable()
        {
            ScreenBroughtDown?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Used to return the pinned screen to its default position
        /// above the toolbar (HMD-only).
        /// </summary>
        public void ResetScreenPositionToOrigin()
        {
            transform.parent = startParent;
            SetLocalPosition(startPosition);
            SetLocalRotation(Quaternion.Euler(0, 0, 0));
        }
    }
}