using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// A lock to allow access to a critical section of code from one coroutine at a time only.
    /// </summary>
    /// <remarks>
    /// Coroutines in Unity can execute from the same thread, so C# constructs like <c>lock</c> do not work for
    /// preventing simultaneous access to critical sections of code.
    /// </remarks>
    public class CoroutineLock
    {
        private readonly WaitForLock waitForLock = new WaitForLock();

        /// <summary>
        /// Allows access to the provided synchronous <c>Action</c> from only one coroutine at a time.
        ///
        /// Do not pass an asynchronous action to this method, use <see cref="CriticalSectionAsync"/>.
        /// </summary>
        /// <param name="action">A synchronous action to lock access to.</param>
        /// <returns></returns>
        public IEnumerator CriticalSection(Action action)
        {
            yield return waitForLock;

            try
            {
                action();
            }
            finally
            {
                waitForLock.Reset();
            }
        }

        /// <summary>
        /// Allows access to the provided asynchronous function from only one coroutine at a time.
        /// </summary>
        /// <param name="function">An asynchronous function to lock access to.</param>
        /// <returns></returns>
        public IEnumerator CriticalSectionAsync(Func<UniTask> function)
        {
            yield return waitForLock;

            yield return function().ToCoroutine();

            waitForLock.Reset();
        }

        private class WaitForLock : CustomYieldInstruction
        {
            private bool locked;

            public override bool keepWaiting
            {
                get
                {
                    if (locked)
                    {
                        return true;
                    }

                    locked = true;
                    return false;
                }
            }

            public override void Reset()
            {
                locked = false;
            }
        }
    }
}