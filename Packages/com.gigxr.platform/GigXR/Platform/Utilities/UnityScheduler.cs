using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// UnityScheduler provides central access to the main Unity thread and scheduler.
    /// </summary>
    /// <remarks>
    /// This can be useful when needing to call Unity methods from a <c>Task</c> as tasks can be scheduled on other
    /// threads.
    /// </remarks>
    public class UnityScheduler : IUnityScheduler
    {
        // --- Static Accessors:

        /// <summary>
        /// The Singleton instance of <c>UnityScheduler</c>.
        /// </summary>
        public static UnityScheduler Instance { get; } = new UnityScheduler();

        // --- Public Properties:

        /// <summary>
        /// The main Unity thread.
        /// </summary>
        public Thread MainThread { get; private set; }

        /// <summary>
        /// A task scheduler for the main Unity thread.
        /// </summary>
        public TaskScheduler MainTaskScheduler { get; private set; }
        
        /// <summary>
        /// A TaskFactory used to schedule work on the main Unity thread.
        /// </summary>
        public TaskFactory MainThreadTaskFactory { get; private set; }

        // --- Unity Methods:

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Instance.MainThread = Thread.CurrentThread;
            Instance.MainTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Instance.MainThreadTaskFactory = new TaskFactory(Instance.MainTaskScheduler);
        }

        // --- Public Methods:

        /// <summary>
        /// Checks if the current thread is the main Unity thread.
        /// </summary>
        /// <returns>true if the main thread; false otherwise.</returns>
        public bool IsMainThread()
        {
            return MainThread == Thread.CurrentThread;
        }

        /// <summary>
        /// Checks if the current thread is the main Unity thread or throw an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">If called not from the main thread.</exception>
        public void EnsureMainThreadOrThrow()
        {
            if (!IsMainThread())
            {
                throw new InvalidOperationException("This operation must be called on the main Unity thread!");
            }
        }

        /// <summary>
        /// Asynchronous version of GetComponent{T} that is safe to call from any thread.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the Component for.</param>
        /// <typeparam name="TComponent">The Component to find.</typeparam>
        /// <returns>The Component on the GameObject or default.</returns>
        public async Task<TComponent> GetComponentTaskSafeAsync<TComponent>(GameObject gameObject)
        {
            try
            {
                return await MainThreadTaskFactory.StartNew(gameObject.GetComponent<TComponent>);
            }
            catch (UnityException exception)
            {
                Debug.LogException(exception);
                return default;
            }
        }

        // --- Private Methods:
    }
}