using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateControllerFinal : MonoBehaviour
{
    private Animator animator;
    private float speed = 0f;
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isTurningLeft = false;
    private bool isTurningRight = false;
    private bool isWalkingBackwards = false;

    void Start()
    {
       animator = GetComponent<Animator>();
        
        // Initialize
        animator.SetFloat("speed", 0f);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isTurningLeft", false);
        animator.SetBool("isTurningRight", false);
        animator.SetBool("isWalkingBackwards", false);


        Debug.Log("Start: speed set to 0, isWalking set to false, isRunning set to false, isTurningLeft set to false, isTurningRight set to false, isWalkingBackwards set to false");
    }

    // Update is called once per frame
    void Update()
    {
       // Detect player input
    bool wKeyPressed = Input.GetKey(KeyCode.W);
    bool shiftKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    bool aKeyPressed = Input.GetKey(KeyCode.A);
    bool dKeyPressed = Input.GetKey(KeyCode.D);

    // Determine speed and movement direction
    if (wKeyPressed)
    {
        isWalking = true;
        isWalkingBackwards = false;
        speed = shiftKeyPressed ? 1.0f : 0.5f;

        // If "W" is pressed and shift is also pressed, the player is running
        isRunning = shiftKeyPressed;
    }
    else
    {
        isWalking = false;
        isWalkingBackwards = false;
        speed = 0f;

        // If "W" is not pressed, the player is not running
        isRunning = false;
    }

    // where are we turning? 
    if (aKeyPressed)
    {
        isTurningLeft = true;
        isTurningRight = false;
    }
    else if (dKeyPressed)
    {
        isTurningLeft = false;
        isTurningRight = true;
    }
    else
    {
        isTurningLeft = false;
        isTurningRight = false;
    }

    // Update Animator para
    animator.SetFloat("speed", speed);
    animator.SetBool("isWalking", isWalking);
    animator.SetBool("isRunning", isRunning);
    animator.SetBool("isTurningLeft", isTurningLeft);
    animator.SetBool("isTurningRight", isTurningRight);
    animator.SetBool("isWalkingBackwards", isWalkingBackwards);

    // Debug help
    Debug.Log("Update: speed set to " + speed);
    Debug.Log("Update: isWalking set to " + isWalking);
    Debug.Log("Update: isRunning set to " + isRunning);
    Debug.Log("Update: isTurningLeft set to " + isTurningLeft);
    Debug.Log("Update: isTurningRight set to " + isTurningRight);
    Debug.Log("Update: isWalkingBackwards set to " + isWalkingBackwards);
    }
}
