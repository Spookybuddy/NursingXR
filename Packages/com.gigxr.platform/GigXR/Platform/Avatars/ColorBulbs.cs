using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Avatars
{
    /// <summary>
    /// Put this scrip on glass bulb objects on head and right hand (left hand is a negative version of right)
    /// </summary>
    public class ColorBulbs : MonoBehaviour
    {
        public Material blue;
        public Material green;
        public Material purple;
        public Material cyan;
        public Material red;
        public Material yellow;
        public Material darkBlue;

        public GameObject rightHandBulb;
        public GameObject leftHandBulb;

        int colorMeThis;

        void Start()
        {
            colorMeThis = Random.Range(1, 7);

            // Get the Renderer component 
            var glassBulb = gameObject.GetComponent<Renderer>();
            var rightHand = rightHandBulb.GetComponent<Renderer>();
            var leftHand = leftHandBulb.GetComponent<Renderer>();

            // Assign colors based on predetermined material colors and a random int
            if (colorMeThis == 1)
            {
                glassBulb.material = blue;
                rightHand.material = blue;
                leftHand.material = blue;
            }
            else

            if (colorMeThis == 2)
            {
                glassBulb.material = green;
                rightHand.material = green;
                leftHand.material = green;
            }
            else if (colorMeThis == 3)
            {
                glassBulb.material = purple;
                rightHand.material = purple;
                leftHand.material = purple;
            }
            else if (colorMeThis == 4)
            {
                glassBulb.material = cyan;
                rightHand.material = cyan;
                leftHand.material = cyan;
            }
            else if (colorMeThis == 5)
            {
                glassBulb.material = red;
                rightHand.material = red;
                leftHand.material = red;

            }
            else if (colorMeThis == 6)
            {
                glassBulb.material = yellow;
                rightHand.material = yellow;
                leftHand.material = yellow;

            }
            else if (colorMeThis == 7)
            {
                glassBulb.material = darkBlue;
                rightHand.material = darkBlue;
                leftHand.material = darkBlue;
            }
        }
    }
}