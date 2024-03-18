using GIGXR.Platform.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GIGXR.Platform.Utilities
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Creates a continuation that executes on the same thread when the target <see cref="Task"/> completes.
        /// </summary>
        /// <remarks>
        /// In Unity, many of the built in methods are not thread-safe. For example, calling PlayerPrefs from a task
        /// will fail if the task gets scheduled not on the main thread. This extension method can be used to schedule
        /// such a task to be executed on the same thread as the caller.
        /// 
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has completed,
        /// whether it completes due to running to completion successfully, faulting due to an unhandled exception, or
        /// exiting out early due to being canceled.
        /// </remarks>
        /// <param name="task">The target task that must complete before the delegate is executed.</param>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task"/> completes.  When run, the delegate will be passed the completed
        /// task as an argument.
        /// </param>
        /// <typeparam name="T">
        /// The type of the result passed to the continuation action.
        /// </typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        public static void ContinueWithOnSameThread<T>(this Task<T> task, Action<Task<T>> continuationAction)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            task.ContinueWith(continuationAction, taskScheduler);
        }

        /// <summary>
        /// Creates a continuation that executes on the same thread when the target <see cref="Task"/> completes.
        /// </summary>
        /// <remarks>
        /// In Unity, many of the built in methods are not thread-safe. For example, calling PlayerPrefs from a task
        /// will fail if the task gets scheduled not on the main thread. This extension method can be used to schedule
        /// such a task to be executed on the same thread as the caller.
        /// 
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has completed,
        /// whether it completes due to running to completion successfully, faulting due to an unhandled exception, or
        /// exiting out early due to being canceled.
        /// </remarks>
        /// <param name="task">The target task that must complete before the delegate is executed.</param>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task"/> completes.  When run, the delegate will be passed the completed
        /// task as an argument.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        public static void ContinueWithOnSameThread(this Task task, Action<Task> continuationAction)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            task.ContinueWith(continuationAction, taskScheduler);
        }

        /// <summary>
        /// Creates a continuation that executes on the main Unity thread when the target <see cref="Task"/> completes.
        /// </summary>
        /// <see cref="ContinueWithOnSameThread{TResult}"/>
        public static void ContinueWithOnUnityThread<T>(this Task<T> task, Action<Task<T>> continuationAction)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            var taskScheduler = UnityScheduler.Instance.MainTaskScheduler;
            task.ContinueWith(continuationAction, taskScheduler);
        }


        /// <summary>
        /// Creates a continuation that executes on the main Unity thread when the target <see cref="Task"/> completes.
        /// </summary>
        /// <see cref="ContinueWithOnSameThread"/>
        public static void ContinueWithOnUnityThread(this Task task, Action<Task> continuationAction)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            var taskScheduler = UnityScheduler.Instance.MainTaskScheduler;
            task.ContinueWith(continuationAction, taskScheduler);
        }

        /// <summary>
        /// Creates a continuation that executes on the main Unity thread when the target <see cref="Task"/> completes. Can be 
        /// cancelled via a cancellation token.
        /// </summary>
        /// <see cref="ContinueWithOnSameThread"/>
        public static void ContinueWithOnUnityThread(this Task task, Action<Task> continuationAction, CancellationToken token)
        {
            if (continuationAction == null)
            {
                throw new ArgumentNullException(nameof(continuationAction));
            }

            var taskScheduler = UnityScheduler.Instance.MainTaskScheduler;
            task.ContinueWith(continuationAction, token, TaskContinuationOptions.LazyCancellation, taskScheduler);
        }

        /// <summary>
        /// Allows an action to be repeated on the Unity Thread.
        /// 
        /// This is not a Task Extension method, but just a Task utility tool.
        /// </summary>
        /// <param name="action">The action to be repeated</param>
        /// <param name="milliseconds">How long between each execution in milliseconds</param>
        /// <param name="token">A token so that the action can be cancelled</param>
        public static void RepeatActionOnUnityThread(Action<Task> action, int milliseconds, CancellationToken token)
        {
            if (action == null)
            {
                UnityEngine.Debug.LogWarning("Cannot repeat a null action on the Unity thread.");
                return;
            }

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // Simple task that allows us to send the action to the Unity Thread
                    PushActionToUnityThread().ContinueWithOnUnityThread(action);

                    await Task.Delay(milliseconds, token);
                }
            }, token);
        }

        private static Task PushActionToUnityThread()
        {
            return Task.CompletedTask;
        }
    }
}