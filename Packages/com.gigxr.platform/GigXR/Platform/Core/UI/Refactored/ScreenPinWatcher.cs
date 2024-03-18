// using Microsoft.MixedReality.Toolkit.UI;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class ScreenPinWatcher : MonoBehaviour
// {
//     private ObjectManipulator[] manipulators;
//
//     void Awake()
//     {
//         manipulators = GetComponentsInChildren<ObjectManipulator>(true);
//
//         foreach(var manip in manipulators)
//         {
//             manip.OnManipulationEnded.AddListener(OnManipulationEnded);
//         }
//     }
//
//     void OnDestroy()
//     {
//         foreach (var manip in manipulators)
//         {
//             manip.OnManipulationEnded.RemoveListener(OnManipulationEnded);
//         }
//     }
//
//     private void OnManipulationEnded(ManipulationEventData t)
//     {
//         if (!Physics.autoSimulation)
//         {
//             Physics.SyncTransforms();
//         }
//     }
// }
