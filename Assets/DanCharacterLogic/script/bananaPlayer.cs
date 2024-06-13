using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class bananaPlayer : MonoBehaviour
{
    PlayerControls controls;
    Vector2 moveInput;
    float growthFactor = 1.0f;
    bool canGrow = true;
    float growthCooldown = 0f;
    public float movementSpeed = 5f;
    public float dashSpeedBoost = 10f;
    public float dashDuration = 0.5f;
    private float originalMovementSpeed;
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsuleCollider;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float growthDuration = 2f;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private LineRenderer lineRenderer;


    Coroutine scaleCoroutine;
    Coroutine emitRaycastCoroutine;
    Coroutine dashCoroutine;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += _ => moveInput = Vector2.zero;
        controls.Gameplay.Grow.performed += ctx =>
        {
            Grow();
            ResetScaleCoroutine();
            Debug.Log("Grow: Performing action");
        };
        controls.Gameplay.Slash.performed += ctx =>
        {
            Slash();
            Debug.Log("Slash: Performing action");
        };

        controls.Gameplay.Dash.performed += _ =>
        {
            Dash();
            Debug.Log("Dash: Performing action");
        };

        controls.Gameplay.BoomerangThrow.performed += ctx =>
        {
            // Trigger the BoomerangThrow animation
            animator.SetTrigger("BoomerangThrow");
            animator.SetBool("Throwing", true);

            if (emitRaycastCoroutine != null)
            {
                StopCoroutine(emitRaycastCoroutine);
            }
            emitRaycastCoroutine = StartCoroutine(EmitRaycast());
            Debug.Log("BoomerangThrow: Performing action");
            Debug.Log("New kill");
        };

        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Set up the LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        capsuleCollider.center = new Vector3(0f, capsuleCollider.height / 2f, 0f);
        originalMovementSpeed = movementSpeed;
        Debug.Log("Start: Initialization complete");
    }

    void OnEnable()
    {
        if (controls != null)
            controls.Gameplay.Enable();
        Debug.Log("OnEnable: Controls enabled");
    }

    void OnDisable()
    {
        if (controls != null)
            controls.Gameplay.Disable();
        Debug.Log("OnDisable: Controls disabled");
    }

    void FixedUpdate()
    {
        Move();
        ApplyGravity();
        AdjustColliderHeight();
        Debug.Log("FixedUpdate: Grounded: " + IsGrounded());
        Debug.Log("FixedUpdate: Position: " + transform.position);
    }

    void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * movementSpeed * growthFactor * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(movement));

        float moveSpeed = moveInput.magnitude;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsMoving", moveSpeed > 0);
        animator.SetBool("IsRunning", moveSpeed > 0.5f);
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
        Debug.Log("Move: Moving with speed " + moveSpeed);
    }

    void Grow()
    {
        if (canGrow && growthCooldown <= 0f)
        {
            growthFactor *= 1.1f;
            transform.localScale *= 1.1f;
            Debug.Log("Grow: Growth factor increased to " + growthFactor);

            // Set cooldown before allowing growth again
            canGrow = false;
            growthCooldown = 3f; // Shortened cooldown time
            Debug.Log("Grow: Cooldown started (" + growthCooldown + " seconds)");

            ResetScaleCoroutine();
        }
        else
        {
            Debug.Log("Grow: Cannot grow yet. Cooldown remaining: " + growthCooldown.ToString("F1") + " seconds");
        }
    }

    void Slash()
    {
        animator.SetTrigger("Slash");
    }

    void HandleInput(InputAction.CallbackContext ctx)
    {
        if (emitRaycastCoroutine != null)
        {
            StopCoroutine(emitRaycastCoroutine);
        }
        emitRaycastCoroutine = StartCoroutine(EmitRaycast());
        Debug.Log("HandleInput: Action coroutine started");
    }

    IEnumerator EmitRaycast()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Debug.Log("EmitRaycast: Started raycasting for " + duration + " seconds");

        // Enable LineRenderer
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, laserOrigin.position);

        while (elapsedTime < duration)
        {
            RaycastHit hit;
            Vector3 rayOrigin = laserOrigin.position;
            Vector3 rayDirection = laserOrigin.forward;

            // Perform raycast
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance))
            {
                lineRenderer.SetPosition(1, hit.point);

                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("EmitRaycast: Player hit! It's a kill!");
                    Destroy(hit.collider.gameObject); // Adjust this line as needed
                }
            }
            else
            {
                // If raycast doesn't hit anything, draw line to max distance
                lineRenderer.SetPosition(1, rayOrigin + rayDirection * raycastDistance);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable LineRenderer when raycasting ends
        lineRenderer.enabled = false;
        Debug.Log("EmitRaycast: Raycasting ended after " + duration + " seconds");
    }


    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
            Debug.Log("ApplyGravity: Applying gravity");
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            Debug.Log("ApplyGravity: Grounded, resetting vertical velocity");
        }
    }

    bool IsGrounded()
    {
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.1f;
        bool grounded = Physics.Raycast(raycastOrigin, Vector3.down, out hit, capsuleCollider.height / 2f, groundLayer);
        Debug.Log("IsGrounded: " + grounded);
        return grounded;
    }

    void AdjustColliderHeight()
    {
        capsuleCollider.height = 2.02f * growthFactor;
        Debug.Log("AdjustColliderHeight: Adjusted height to " + capsuleCollider.height);
    }

    void ResetScaleCoroutine()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleDownAfterDelay());
        Debug.Log("ResetScaleCoroutine: Resetting scale coroutine");
    }

    IEnumerator ScaleDownAfterDelay()
    {
        yield return new WaitForSeconds(growthDuration);

        while (growthFactor > 1.0f)
        {
            growthFactor -= Time.deltaTime / growthDuration;
            transform.localScale *= 1.0f - Time.deltaTime / growthDuration;
            yield return null;
        }

        growthFactor = 1.0f;
        transform.localScale = Vector3.one;

        // Reset growth ability
        canGrow = true;
        Debug.Log("ScaleDownAfterDelay: Scaling down complete");
    }

    public void Dash()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
        }
        dashCoroutine = StartCoroutine(DashEffect());
        Debug.Log("Dash: Dashing started");
    }

    IEnumerator DashEffect()
    {
        // Calculate the destination position 2 meters forward
        Vector3 destination = transform.position + transform.forward * 2f;

        // Move the player to the destination position
        rb.MovePosition(destination);

        // Wait for a short duration
        yield return new WaitForSeconds(dashDuration); // Adjust this duration as needed

        // Ensure the player's velocity is reset to prevent unwanted movement
        rb.velocity = Vector3.zero;
    }
}
