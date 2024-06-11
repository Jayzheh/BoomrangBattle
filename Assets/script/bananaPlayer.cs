using UnityEngine;
using UnityEngine.InputSystem;

public class BananaPlayer : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    float rotate;
    public GameObject boomer;
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
    [SerializeField] private BoomWeap boomWeap; // Reference to the BoomWeap script
    [SerializeField] private GameObject handPositionGameObject;

    void Start()
    {
        // Initialize PlayerControls
        controls = new PlayerControls();

        // Add event listeners for input actions
        controls.Gameplay.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        controls.Gameplay.Move.canceled += ctx => OnMove(Vector2.zero);
        controls.Gameplay.Rotate.performed += ctx => OnRotate(ctx.ReadValue<float>());
        controls.Gameplay.Rotate.canceled += ctx => OnRotate(0f);


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

        Debug.Log("BananaPlayer Start method executed");
    }

    void OnEnable()
    {
        if (controls != null)
        {
            controls.Gameplay.Enable();
            Debug.Log("Controls enabled");
        }
    }

    void OnDisable()
    {
        if (controls != null)
        {
            controls.Gameplay.Disable();
            Debug.Log("Controls disabled");
        }
    }

    // Method to handle the input action for throwing the boomerang
    public void OnThrowBoomerang(InputAction.CallbackContext context)
    {
        // Check if the action is performed (button is pressed)
        if (context.started)
        {
            // Call the ThrowBoomerang method of the BoomWeap component
            boomWeap.ThrowBoomerang();
        }
    }

    void Update()
    {
        UpdatePlayer();
    }



    void FixedUpdate()
    {
        FixedUpdatePlayer();
    }

    void OnMove(Vector2 movement)
    {
        move = movement;
        Debug.Log("Move Input: " + movement);
    }

    void OnRotate(float rotation)
    {
        rotate = rotation;
         Debug.Log("Rotate Input: " + rotation);
    }

    void UpdatePlayer()
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
void FixedUpdatePlayer()
{
    // Only apply movement and rotation if the character is not throwing the boomerang
    if (!boomWeap.isBoomerangThrown)
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
}


}
