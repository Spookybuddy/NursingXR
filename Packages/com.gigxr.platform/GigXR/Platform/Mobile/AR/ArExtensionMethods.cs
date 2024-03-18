using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace GIGXR.Platform.Mobile.AR
{
    public static class ArExtensionMethods
    {
        /// <summary>
        /// Calculate the current plane area
        /// </summary>
        /// <param name="plane"></param>
        /// <returns>Return the current plane size</returns>
        public static float CalculatePlaneArea(this ARPlane plane)
        {
            return plane.size.x * plane.size.y;
        }
    }
}