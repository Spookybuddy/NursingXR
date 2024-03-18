using UnityEngine;

namespace GIGXR.Platform.Core
{
    /// <summary>
    /// This component should be inherited and the type specified should be one of the dependency interfaces in GIGXRCore.
    /// The inherited component should then be placed on the same GameObject as the GIGXRCore component in your scene. When
    /// the GIGXRCore starts building all the dependency, it will check to see if there are any overrides and use those over
    /// the default classes.
    /// </summary>
    [RequireComponent(typeof(GIGXRCore))]
    public abstract class CoreInjectorComponent<T> : MonoBehaviour
    {
        public abstract T GetSingleton();
    }
}