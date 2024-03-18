using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace GIGXR.Platform.Mobile.UI
{
    public enum PhoneScreenType
    {
        ExtraSmall,
        Small,
        Medium,
        Large,
        ExtraLarge,
        iPad,
        iPadPro,
        iPadProXL
    }

//Script for setting Canvas Scale factor based on device screen width
    public class ScreenSizeCheck : MonoBehaviour
    {
        public static PhoneScreenType PhoneScreenType;

        public CanvasScaler CanvasScaler { get; private set; }

        [SerializeField] private float extraSmallScaleFactor,
            smallScreenScaleFactor,
            mediumScreenScaleFactor,
            largeScreenScaleFactor,
            extraLargeScaleFactor,
            iPadScaleFactor,
            iPadProScaleFactor,
            iPadProXLScaleFactor;

        [SerializeField] private int extraSmallWidth = 640,
            smallWidth = 750,
            mediumWidth = 828,
            largeScreenWidth = 1080,
            extraLargeWidth = 1242,
            iPadWidth = 1536,
            iPadProWidth = 1668,
            iPadProXLWidth = 2048;

        void Awake()
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());

            CanvasScaler = GetComponent<CanvasScaler>();

            print($"Screen width is {Screen.width}");

            if (Screen.width >= iPadProXLWidth)
            {
                PhoneScreenType = PhoneScreenType.iPadProXL;
            }
            else if (Screen.width >= iPadProWidth && Screen.width <= iPadProXLWidth)
            {
                PhoneScreenType = PhoneScreenType.iPadPro;
            }
            else if (Screen.width >= iPadWidth && Screen.width <= iPadProWidth)
            {
                PhoneScreenType = PhoneScreenType.iPad;
            }
            else if (Screen.width >= extraLargeWidth && Screen.width <= iPadWidth)
            {
                PhoneScreenType = PhoneScreenType.ExtraLarge;
            }
            else if (Screen.width <= extraLargeWidth && Screen.width >= largeScreenWidth)
            {
                PhoneScreenType = PhoneScreenType.Large;
            }
            else if (Screen.width <= largeScreenWidth && Screen.width >= mediumWidth)
            {
                PhoneScreenType = PhoneScreenType.Medium;
            }
            else if (Screen.width <= mediumWidth && Screen.width >= smallWidth)
            {
                PhoneScreenType = PhoneScreenType.Small;
            }
            else if (Screen.width <= extraSmallWidth)
            {
                PhoneScreenType = PhoneScreenType.ExtraSmall;
            }

            switch (PhoneScreenType)
            {
                case (PhoneScreenType.ExtraSmall):
                    print("Extra small screen");
                    CanvasScaler.scaleFactor = extraSmallScaleFactor;
                    break;
                case (PhoneScreenType.Small):
                    print("Small Screen");
                    CanvasScaler.scaleFactor = smallScreenScaleFactor;
                    break;
                case (PhoneScreenType.Medium):
                    print("Mediumn Screen");
                    CanvasScaler.scaleFactor = mediumScreenScaleFactor;
                    break;
                case (PhoneScreenType.Large):
                    print("Large Screen");
                    CanvasScaler.scaleFactor = largeScreenScaleFactor;
                    break;
                case (PhoneScreenType.ExtraLarge):
                    print("Extra Large Screen");
                    CanvasScaler.scaleFactor = extraLargeScaleFactor;
                    break;
                case (PhoneScreenType.iPad):
                    print("iPad Screen");
                    CanvasScaler.scaleFactor = iPadScaleFactor;
                    break;
                case (PhoneScreenType.iPadPro):
                    print("iPadPro Screen");
                    CanvasScaler.scaleFactor = iPadProScaleFactor;
                    break;
                case (PhoneScreenType.iPadProXL):
                    print("iPadProXL Screen");
                    CanvasScaler.scaleFactor = iPadProXLScaleFactor;
                    break;
            }

            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());
        }
    }
}