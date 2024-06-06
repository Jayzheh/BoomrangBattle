using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        //if player presses w key
        if (Input.GetKey("w"))
        {
            //set is walking to true 
            animator.SetBool("IsWalking", true);
        }
        else
        {
            //if w key is not pressed, set isWalking to false
            animator.SetBool("IsWalking", false);
        }

    }
}
