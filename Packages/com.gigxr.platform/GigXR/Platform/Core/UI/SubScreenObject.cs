using UnityEngine;
using GIGXR.Platform.Interfaces;

namespace GIGXR.Platform.UI
{
    using Core.DependencyValidator;
    using System;

    /// <summary>
    /// Class to enable or disable gameobjects depending on the parent screenobject's substate.
    /// </summary>
    public class SubScreenObject : UiObject
    {
        public SubScreenState SubState { get { return activeSubScreenState; } }

        [RequireDependency]
        [SerializeField]
        private SubScreenState activeSubScreenState;
    }
}