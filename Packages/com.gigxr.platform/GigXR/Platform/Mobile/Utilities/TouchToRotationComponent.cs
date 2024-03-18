using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchToRotationComponent : MonoBehaviour
{
    public float rotatespeed = 10f;

    private float lastXPosition;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    lastXPosition = touch.position.x;
                    break;
                case TouchPhase.Moved:
                    transform.Rotate(transform.up, (lastXPosition - touch.position.x) * rotatespeed * Time.deltaTime);
                    
                    lastXPosition = touch.position.x;
                    break;
                case TouchPhase.Ended:
                    Debug.Log("Touch Phase Ended.");
                    break;
            }
        }
    }
}