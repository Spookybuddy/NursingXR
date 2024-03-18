namespace GIGXR.Platform.Utilities
{
    using System.Threading.Tasks;
    using UnityEngine;

    public interface IUnityScheduler
    {
        /// <summary>
        /// Asynchronous version of GetComponent{T} that is safe to call from any thread.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the Component for.</param>
        /// <typeparam name="TComponent">The Component to find.</typeparam>
        /// <returns>The Component on the GameObject or default.</returns>
        public Task<TComponent> GetComponentTaskSafeAsync<TComponent>(GameObject gameObject);
    }
}