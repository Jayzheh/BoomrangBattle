<<<<<<< HEAD
using UnityEngine.InputSystem;
using UnityEngine;

public class NewPlayer : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    float rotate;
    public float speed = 5f;
    public Animator animator;
    public float walkSpeedThreshold = 0.1f;
    public float runSpeedThreshold = 0.5f;
    public Camera cam;
    private Rigidbody rb;
    public float turnSpeed = 100f;
    [SerializeField]
    private LayerMask groundLayer;

    private CapsuleCollider capsuleCollider;

    void Awake()
    {
        // Print the initial position for debugging
        Debug.Log("Initial position: " + transform.position);

        // Initialize PlayerControls
        controls = new PlayerControls();

        // Add event listeners for input actions
        controls.Gameplay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => move = Vector2.zero;
        controls.Gameplay.Rotate.performed += ctx => rotate = ctx.ReadValue<float>();
        controls.Gameplay.Rotate.canceled += ctx => rotate = 0f;

        // Add event listeners for BoomerangThrow and Slash actions
        controls.Gameplay.BoomerangThrow.performed += ctx => TriggerBoomerangThrow();
        controls.Gameplay.Slash.performed += ctx => TriggerSlash();

        // Get the Rigidbody and Collider components of the character
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Ensure char is upright
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Optional: Adjust the mass if needed
        rb.mass = 1f; // Adjust based on your requirements

        // Set the height of the capsule collider
        capsuleCollider.height = 1.6f;

        rb.useGravity = false;
=======
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class bananaPlayer : MonoBehaviour
{
     PlayerControls controls;
    Vector2 moveInput;
    float growthFactor = 1.0f;
    public float movementSpeed = 5f;
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsuleCollider; // Reference to the character's capsule collider

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float growthDuration = 2f; // Duration for growth in seconds

    Coroutine scaleCoroutine; // Coroutine reference for scaling back down

    void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += _ => moveInput = Vector2.zero;
        controls.Gameplay.Grow.performed += ctx =>
        {
            Grow();
            ResetScaleCoroutine(); // Reset coroutine when player grows
        };
        controls.Gameplay.Slash.performed += ctx => Slash();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true; // Enable gravity

        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // Get the capsule collider component

        // Adjust the initial position of the capsule collider to match the initial position of the character
        capsuleCollider.center = new Vector3(0f, capsuleCollider.height / 2f, 0f);
>>>>>>> origin/boomerang
    }

    void OnEnable()
    {
<<<<<<< HEAD
        // Initialize controls if they are not already initialized
        if (controls == null)
        {
            controls = new PlayerControls();
        }
        // Enable PlayerControls
        controls.Gameplay.Enable();
=======
        if (controls != null)
            controls.Gameplay.Enable();
>>>>>>> origin/boomerang
    }

    void OnDisable()
    {
<<<<<<< HEAD
        // Ensure controls are initialized before disabling
        if (controls != null)
        {
            // Disable PlayerControls
            controls.Gameplay.Disable();
        }
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

        // Ground Check
        if (IsGrounded())
        {
            // Grounded, do nothing
        }
        else
        {
            // Apply a small force or velocity opposite to gravity to prevent falling further
            rb.AddForce(Vector3.up * 0.1f, ForceMode.Impulse);
=======
        if (controls != null)
            controls.Gameplay.Disable();
    }

    void FixedUpdate()
    {
        Move();
        ApplyGravity();
        AdjustColliderHeight();
        Debug.Log("Grounded: " + IsGrounded());
        Debug.Log("Position: " + transform.position);
    }

    void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * movementSpeed * growthFactor * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(movement));

        // Update animator parameters
        float moveSpeed = moveInput.magnitude;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsMoving", moveSpeed > 0);
        animator.SetBool("IsRunning", moveSpeed > 0.5f);
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
    }

    void Grow()
    {
        growthFactor *= 1.5f;
        transform.localScale *= 1.5f;
    }

    void Slash()
    {
        animator.SetTrigger("Slash");
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
        else
        {
            // If grounded, reset vertical velocity to prevent bouncing
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
>>>>>>> origin/boomerang
        }
    }

    bool IsGrounded()
    {
<<<<<<< HEAD
        // Get the position for the raycast origin, slightly above the bottom of the collider
        Vector3 origin = transform.position + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.center.y);

        // Raycast downward to check if the character is grounded
        float raycastDistance = 0.2f + capsuleCollider.height / 2; // Adjust the distance as needed
        bool grounded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer);

        // Debug the raycast to visualize it in the scene view
        Debug.DrawRay(origin, Vector3.down * raycastDistance, grounded ? Color.green : Color.red);

        return grounded;
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

    void TriggerBoomerangThrow()
    {
        // Trigger BoomerangThrow animation here
        animator.SetTrigger("BoomerangThrow");
    }

    void TriggerSlash()
    {
        // Trigger Slash animation here
        animator.SetTrigger("Slash");
=======
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.1f; // Slightly above the ground
        bool grounded = Physics.Raycast(raycastOrigin, Vector3.down, out hit, capsuleCollider.height / 2f, groundLayer);
        return grounded;
    }

    void ResetScaleCoroutine()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleDownAfterDelay());
    }

    IEnumerator ScaleDownAfterDelay()
    {
        yield return new WaitForSeconds(growthDuration);
        while (growthFactor > 1.0f)
        {
            growthFactor -= Time.deltaTime / growthDuration; // Scale back down gradually
            transform.localScale *= 1.0f - Time.deltaTime / growthDuration;
            yield return null;
        }
        growthFactor = 1.0f;
        transform.localScale = Vector3.one; // Ensure scale is exactly 1.0f
    }

    void AdjustColliderHeight()
    {
        // Adjust the capsule collider height based on the growth factor
        capsuleCollider.height = 2.02f * growthFactor; // Adjust this value according to your character's original collider height
>>>>>>> origin/boomerang
    }
}
