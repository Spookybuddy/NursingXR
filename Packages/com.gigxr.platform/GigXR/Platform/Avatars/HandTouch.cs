using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Avatars
{
    public class HandTouch : MonoBehaviour
    {
        [SerializeField] public Animator myHandAnimator;

        [SerializeField] public string isTimeToHold = "isTimeToHold";
        [SerializeField] public string isTimeToLetGo = "isTimeToLetGo";

        private void OnTriggerEnter(Collider thatGrabbedThing)
        {
            if (thatGrabbedThing.tag != "Avatar")
            {
                myHandAnimator.SetBool(isTimeToHold, true);
                myHandAnimator.SetBool(isTimeToLetGo, false);
            }
        }

        private void OnTriggerExit(Collider exitStageLeftThing)
        {
            if (exitStageLeftThing.tag != "Avatar")
            {
                myHandAnimator.SetBool(isTimeToLetGo, true);
                myHandAnimator.SetBool(isTimeToHold, false);
            }
        }
    }
}