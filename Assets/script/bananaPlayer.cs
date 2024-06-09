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
        rb.mass = 2f; // Adjust based on your requirements
    }

    void OnEnable()
    {
        // Initialize controls if they are not already initialized
        if (controls == null)
        {
            controls = new PlayerControls();
        }
        // Enable PlayerControls
        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
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
        }
    }

    bool IsGrounded()
    {
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
    }
}
