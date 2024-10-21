using GIGXR.Platform.Core.DependencyInjection;
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
    [DefaultExecutionOrder(-50)]
    public abstract class CoreInjectorComponent<T> : MonoBehaviour where T : class
    {
        public bool NewSingleton = false;

        public void Awake()
        {
            if (NewSingleton)
            {
                DependencyProvider.RegisterSingleton<T>(_ => GetSingleton());
            }
        }

        public abstract T GetSingleton();

        public IDependencyProvider DependencyProvider
        {
            get
            {
                return Core.DependencyProvider;
            }
        }

        private GIGXRCore Core
        {
            get
            {
                if (_core == null)
                {
                    _core = GetComponent<GIGXRCore>();
                }

                return _core;
            }
        }

        private GIGXRCore _core;
    }
}