﻿/// <summary>
/// write by 52cwalk,if you have some question ,please contract lycwalk@gmail.com
/// </summary>

using UnityEngine;
using System.Collections;

public class CameraPlaneController : MonoBehaviour
{
    public Camera _targetCam;

    ScreenOrientation orientation;
    float height = 0;
    float width = 0;
    float screenRatio = 1.0f;
    private IEnumerator updateCoroutine;
    private WaitForSeconds wfs;

    // Use this for initialization
    void Start()
    {
        // Init();
        // correctPlaneScale(1.0f);

        wfs = new WaitForSeconds(.2f);
        
        // todo: some de-initialisation.
        //StartCoroutine(updateCoroutine = UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return wfs;
            Do();
        }
    }

    private void Init()
    {
        float Screenheight = (float)_targetCam.orthographicSize * 2.0f;
        float Screenwidth = Screenheight * Screen.width / Screen.height;
        height = Screenheight;
        width = Screenwidth;
        
        // this.transform.localPosition = new Vector3(0, 0, 91.6f);

#if UNITY_EDITOR|| UNITY_STANDALONE || UNITY_WSA_10_0 || UNITY_METRO_API || UNITY_WSA
        transform.localEulerAngles = new Vector3(90, 180, 0);
        transform.localScale = new Vector3(width / 10, 1.0f, height / 10);
#elif UNITY_WEBPLAYER
		transform.localEulerAngles = new Vector3(90,180,0);
		transform.localScale = new Vector3(width/10, 1.0f, height/10);
#endif

        orientation = Screen.orientation;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (Screen.orientation == ScreenOrientation.Portrait ||
            Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
#if UNITY_EDITOR|| UNITY_STANDALONE || UNITY_WSA_10_0 || UNITY_METRO_API || UNITY_WSA
            transform.localEulerAngles = new Vector3(90, 180, 0);
            transform.localScale = new Vector3(width / 10, 1.0f, height / 10);

#elif UNITY_ANDROID
			transform.localEulerAngles = new Vector3(0,270,90);
			transform.localScale = new Vector3(height/10, 1.0f, width/10);
#elif UNITY_IOS
			if( Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			{
				transform.localEulerAngles = new Vector3(0,270,90);
			}
			else
			{
				transform.localEulerAngles = new Vector3(0,90,270);
			}
			transform.localScale = new Vector3(-1*height/10, 1.0f, width/10);
#endif
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
#if UNITY_EDITOR|| UNITY_STANDALONE || UNITY_WSA_10_0 || UNITY_METRO_API || UNITY_WSA
            transform.localEulerAngles = new Vector3(90, 180, 0);
            transform.localScale = new Vector3(width / 10, 1.0f, height / 10);

#elif UNITY_ANDROID
			transform.localEulerAngles = new Vector3(90,180,0);
			transform.localScale = new Vector3(width/10, 1.0f, height/10);
#elif UNITY_IOS
			transform.localEulerAngles = new Vector3(-90,0,0);
			transform.localScale = new Vector3(-1*width/10, 1.0f, height/10);
#endif
        }
    }

    // Update is called once per frame
    void Do()
    {
        if (orientation != Screen.orientation)
        {
            int screenHeight_1 = Screen.height;
            int screenWidth_1 = Screen.width;
            if (Screen.orientation == ScreenOrientation.Portrait ||
                Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                if (screenHeight_1 < screenWidth_1)
                {
                    int tempvalue = screenWidth_1;
                    screenWidth_1 = screenHeight_1;
                    screenHeight_1 = tempvalue;
                }

                float Screenheight = (float)_targetCam.orthographicSize * 2.0f;
                float Screenwidth = Screenheight * screenWidth_1 / screenHeight_1;
                height = Screenheight;
                width = Screenwidth;
#if UNITY_ANDROID
				transform.localEulerAngles = new Vector3(0,270,90);
				transform.localScale = new Vector3(height/10, 1.0f, width/10);
#elif UNITY_IOS
				if( Screen.orientation == ScreenOrientation.PortraitUpsideDown)
				{
					transform.localEulerAngles = new Vector3(0,270,90);
				}
				else
				{
					transform.localEulerAngles = new Vector3(0,90,270);
				}

				transform.localScale = new Vector3(-1*height/10, 1.0f, width/10);
#endif
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeLeft ||
                     Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                if (screenHeight_1 > screenWidth_1)
                {
                    int tempvalue = screenWidth_1;
                    screenWidth_1 = screenHeight_1;
                    screenHeight_1 = tempvalue;
                }

                float Screenheight = (float)_targetCam.orthographicSize * 2.0f;
                float Screenwidth = Screenheight * screenWidth_1 / screenHeight_1;
                height = Screenheight;
                width = Screenwidth;

#if UNITY_ANDROID
				transform.localEulerAngles = new Vector3(90,180,0);
				transform.localScale = new Vector3(width/10, 1.0f, height/10);
#elif UNITY_IOS
				transform.localEulerAngles = new Vector3(-90,0,0);
				transform.localScale = new Vector3(-1*width/10, 1.0f, height/10);
#endif
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                if (screenHeight_1 > screenWidth_1)
                {
                    int tempvalue = screenWidth_1;
                    screenWidth_1 = screenHeight_1;
                    screenHeight_1 = tempvalue;
                }

                float Screenheight = (float)_targetCam.orthographicSize * 2.0f;
                float Screenwidth = Screenheight * screenWidth_1 / screenHeight_1;
                height = Screenheight;
                width = Screenwidth;
#if UNITY_ANDROID
				transform.localEulerAngles = new Vector3(-90,0,0);
				transform.localScale = new Vector3(width/10, 1.0f, height/10);
#elif UNITY_IOS
				transform.localEulerAngles = new Vector3(90,180,0);
				transform.localScale = new Vector3(-1*width/10, 1.0f, height/10);
#endif
            }

            orientation = Screen.orientation;
            correctPlaneScale(screenRatio);
        }
    }

    public void correctPlaneScale
    (
        float size
    )
    {
        screenRatio = size;
#if UNITY_ANDROID|| UNITY_IOS
		Vector3 orgVec = transform.localScale;
		
		if(screenRatio >1f)
		{
			transform.localScale = new Vector3(orgVec.x, 1.0f, orgVec.z * screenRatio);	
		}
		else if(screenRatio <1 && screenRatio >0)
		{
			transform.localScale = new Vector3(orgVec.x/screenRatio, 1.0f, orgVec.z);	
		}
		else
		{
		
		}
#endif
    }
}