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
    }

    void OnEnable()
    {
        if (controls != null)
            controls.Gameplay.Enable();
    }

    void OnDisable()
    {
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
        }
    }

    bool IsGrounded()
    {
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
    }
}
