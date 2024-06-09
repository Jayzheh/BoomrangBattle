using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class bananaPlayer : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    Vector2 rotate;
    public float speed = 5f;
    public Animator animator;
    public float walkSpeedThreshold = 0.1f; // Adjust this threshold value as needed
    public float runSpeedThreshold = 0.5f; // Adjust this threshold value as needed
    public Camera cam;
    private Rigidbody rb;

    void Awake()
    {
        // Initialize PlayerControls
        controls = new PlayerControls();

        if (controls != null)
        {
            // Add event listeners for input actions
            controls.Gameplay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            controls.Gameplay.Move.canceled += ctx => move = Vector2.zero;
        }
        else
        {
            Debug.LogError("Failed to initialize controls!");
        }
        
        // Get the Rigidbody component of the character
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Calculate horizontal and vertical movement
        float horizontal = move.x;
        float vertical = move.y;

        // Calculate total movement speed
        float moveSpeed = move.magnitude;

        // Set animator parameters
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsMoving", moveSpeed > 0);
        
        // Transition to running if moving faster than threshold and already running
        animator.SetBool("IsRunning", moveSpeed > runSpeedThreshold && animator.GetBool("IsRunning"));

        // Debug statements
        Debug.Log("Move Speed: " + moveSpeed);
        Debug.Log("Is Moving: " + (moveSpeed > 0));
        Debug.Log("Is Running: " + (moveSpeed > runSpeedThreshold && animator.GetBool("IsRunning")));

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
    }

    void FixedUpdate()
    {
        // Calculate the movement direction based on the input
        Vector3 movement = new Vector3(move.x, 0, move.y);
        movement = transform.TransformDirection(movement);

        // Move the character
        rb.MovePosition(transform.position + movement * speed * Time.deltaTime);
    }

    void OnEnable()
    {
        // Enable PlayerControls
        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        // Disable PlayerControls
        controls.Gameplay.Disable();
    }
}
