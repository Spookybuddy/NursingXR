using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Example class to set some parameters for an Animator.
/// </summary>
/// 
[RequireComponent(typeof(Animator))]
public class AnimationControllerExample : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.I))
        {
            animator.SetFloat("rotateOverXFloat", 10);
        }
        else if(Input.GetKeyUp(KeyCode.K))
        {
            animator.SetFloat("rotateOverXFloat", 0);
        }
    }
}
