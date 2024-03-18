using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Avatars
{
    /// <summary>
    /// Since all the avatars would look silly blinking at the same time, this script loops the same blink animation (that includes the 
    /// pause between blinking, too), but starts it at a random time for each avatar. That should put them out of sync for the most part.  
    /// </summary>
    public class Blink : MonoBehaviour
    {
        [SerializeField]
        public Animator headAnimator;

        private void Start()
        {
            StartCoroutine(BlinkWait(Random.Range(1, 30)));
        }

        IEnumerator BlinkWait(int waitThisLongToBlink)
        {
            yield return new WaitForSeconds(waitThisLongToBlink);

            headAnimator.SetBool("IsBlinkTime", true);
        }
    }
}