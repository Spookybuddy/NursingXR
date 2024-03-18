using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    public class CoroutineUtil : MonoBehaviour
    {
        public static IEnumerator DelayedAction(float delayInSeconds, Action action)
        {
            yield return new WaitForSeconds(delayInSeconds);

            action.Invoke();
        }
    }
}
