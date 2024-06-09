using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class bananaPlayer : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    float rotate;
    public float speed = 5f;
    public Animator animator;
    public float walkSpeedThreshold = 0.1f; // Adjust this threshold value as needed
    public float runSpeedThreshold = 0.5f; // Adjust this threshold value as needed
    public Camera cam;
    private Rigidbody rb;
    public float turnSpeed = 100f; // Speed of turning

    void Awake()
    {
        // Initialize PlayerControls
        controls = new PlayerControls();

        if (controls != null)
        {
            // Add event listeners for input actions
            controls.Gameplay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            controls.Gameplay.Move.canceled += ctx => move = Vector2.zero;
            controls.Gameplay.Rotate.performed += ctx => rotate = ctx.ReadValue<float>();
            controls.Gameplay.Rotate.canceled += ctx => rotate = 0f;
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

        // Transition to running if moving faster than threshold
        animator.SetBool("IsRunning", moveSpeed > runSpeedThreshold);

        // Set horizontal and vertical parameters
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        // Debug statements
        Debug.Log("Move Speed: " + moveSpeed);
        Debug.Log("Is Moving: " + (moveSpeed > 0));
        Debug.Log("Is Running: " + (moveSpeed > runSpeedThreshold));
        Debug.Log("Horizontal: " + horizontal);
        Debug.Log("Vertical: " + vertical);
    }

    void FixedUpdate()
    {
        // Calculate the movement direction based on the input
        Vector3 movement = transform.forward * move.y * speed * Time.deltaTime;

        // Move the character
        rb.MovePosition(rb.position + movement);

        // Rotate the character
        float turn = rotate * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Rotate the camera if needed
        if (cam != null)
        {
            cam.transform.Rotate(new Vector3(0, rotate, 0) * turnSpeed * Time.deltaTime);
        }
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
