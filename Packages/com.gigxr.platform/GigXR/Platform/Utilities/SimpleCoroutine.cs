using Logger = GIGXR.Platform.Utilities.Logger;

using System;
using System.Collections;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Simple Coroutine wrapper for a common Coroutine use case, in which a single routine is toggled "on" or "off"
    /// and should only have 1 running instance at a time.
    /// </summary>
    public class SimpleCoroutine
    {
        private bool routineIsRunning = false;
        public bool RoutineIsRunning => routineIsRunning;

        private MonoBehaviour owner;
        private Coroutine routine;
        private Func<IEnumerator> enumeratorProvider;
        private Action onRoutineEnd;

        public SimpleCoroutine(MonoBehaviour owner, Func<IEnumerator> enumeratorProvider, Action onCoroutineStop = null)
        {
            this.owner = owner;
            this.enumeratorProvider = enumeratorProvider;
            this.onRoutineEnd = onCoroutineStop;
        }

        public void StartRoutine(bool restartIfRunning = true)
        {
            if (restartIfRunning)
            {
                StopRoutine();
                StartRoutineInner();
            }
            else if (!RoutineIsRunning)
            {
                StartRoutineInner();
            }
        }

        private void StartRoutineInner()
        {
            if (!owner.enabled || !owner.gameObject.activeInHierarchy)
            {
                Logger.Warning("[SimpleCoroutine] Coroutine not started; host monobehavior is inactive.");
                return;
            }

            routineIsRunning = true;
            routine = owner.StartCoroutine(RoutineWrapper(enumeratorProvider()));
        }

        public void StopRoutine()
        {
            if (routine != null)
            {
                owner.StopCoroutine(routine);

                routineIsRunning = false;
                routine = null;
                onRoutineEnd?.Invoke();
            }
        }

        private IEnumerator RoutineWrapper(IEnumerator nestedRoutine)
        {
            while (nestedRoutine.MoveNext())
                yield return nestedRoutine.Current;

            routineIsRunning = false;
            routine = null;
            onRoutineEnd?.Invoke();
            yield return null;
        }
    }
}
