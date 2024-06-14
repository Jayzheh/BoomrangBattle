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
    public float movementSpeed = 9f;
    public float dashSpeedBoost = 10f;
    public float dashDistance = 3f;
    public float dashDuration = 0.1f;
    private float originalMovementSpeed;
    private Animator animator;
    private Rigidbody rb;
    private Collider slashCollider;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float growthDuration = 2f;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private ParticleSystem beamParticleSystem;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject boomerangPrefab;

    Coroutine scaleCoroutine;
    Coroutine emitRaycastCoroutine;
    Coroutine dashCoroutine;

    // Parameters for animator
    float speed;
    bool IsMoving;
    bool IsRunning;
    float Horizontal;
    float Vertical;
    bool BoomerangThrow;
    bool Slash;
    bool Throwing;

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
            PerformSlash();
            Debug.Log("Slash: Performing action");
        };

        controls.Gameplay.Dash.performed += _ =>
        {
            PerformDash();
            Debug.Log("Dash: Performing action");
        };

        controls.Gameplay.BoomerangThrow.performed += ctx =>
        {
            PerformBoomerangThrow();
            Debug.Log("BoomerangThrow: Performing action");
        };

        // Get the slash collider component (assuming it's a child object)
        slashCollider = GetComponentInChildren<Collider>();
        if (slashCollider == null)
        {
            Debug.LogError("Slash collider not found!");
        }

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.useGravity = false;
        }
        else
        {
            Debug.LogError("Rigidbody component not found!");
        }
    }

    void Start()
    {
        originalMovementSpeed = movementSpeed;
        Debug.Log("Start: Initialization complete");
    }

    void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
            // Reassign event callbacks here if needed
        }

        controls.Gameplay.Enable();
        Debug.Log("OnEnable: Controls enabled");
    }

    void OnDisable()
    {
        if (controls != null)
        {
            controls.Gameplay.Disable();
            Debug.Log("OnDisable: Controls disabled");
        }
        else
        {
            Debug.LogWarning("PlayerControls is null in OnDisable!");
        }
    }

    void FixedUpdate()
    {
        Move();
        ApplyGravity();
        Debug.Log("FixedUpdate: Grounded: " + IsGrounded());
        Debug.Log("FixedUpdate: Position: " + transform.position);
    }

    void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * movementSpeed * growthFactor * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + transform.TransformDirection(movement));

        float moveSpeed = moveInput.magnitude;
        speed = moveSpeed;
        IsMoving = moveSpeed > 0;
        IsRunning = moveSpeed > 0.5f;
        Horizontal = moveInput.x;
        Vertical = moveInput.y;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsMoving", IsMoving);
        animator.SetBool("IsRunning", IsRunning);
        animator.SetFloat("Horizontal", Horizontal);
        animator.SetFloat("Vertical", Vertical);

        Debug.Log("Move: Moving with speed " + moveSpeed);
    }

    void Grow()
    {
        if (canGrow && growthCooldown <= 0f)
        {
            growthFactor *= 1.1f;
            transform.localScale *= 1.1f;
            Debug.Log("Grow: Growth factor increased to " + growthFactor);

            canGrow = false;
            growthCooldown = 3f;
            Debug.Log("Grow: Cooldown started (" + growthCooldown + " seconds)");

            ResetScaleCoroutine();
        }
        else
        {
            Debug.Log("Grow: Cannot grow yet. Cooldown remaining: " + growthCooldown.ToString("F1") + " seconds");
        }
    }
    void PerformSlash()
    {
        if (slashCollider != null)
        {
            // Set trigger in animator
            animator.SetTrigger("Slash");
            Debug.Log("Slash: Performing action");
        }
        else
        {
            Debug.LogWarning("Slash: Slash collider not found!");
        }
    }

    IEnumerator EnableSlashCollider()
    {
        slashCollider.enabled = true;
        yield return new WaitForSeconds(0.1f); // Enable for a short duration
        slashCollider.enabled = false;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Slash: Enemy hit! It's a kill!");
            Destroy(other.gameObject);
        }
    }

    void PerformBoomerangThrow()
    {
        if (animator != null)
        {
            animator.SetTrigger("BoomerangThrow");
            Debug.Log("BoomerangThrow: Performing action");
        }
        else
        {
            Debug.LogWarning("BoomerangThrow: Animator is null!");
        }

        // Handle detection using raycast or other logic (as previously implemented)
        if (emitRaycastCoroutine != null)
        {
            StopCoroutine(emitRaycastCoroutine);
        }
        emitRaycastCoroutine = StartCoroutine(EmitRaycast());
    }


    void PerformDash()
    {
        Dash();
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
        // Use the current movement direction if there's input, otherwise use forward direction
        Vector3 dashDirection = moveInput.normalized;
        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        // Calculate target position
        Vector3 targetPosition = transform.position + dashDirection * dashDistance;

        // Perform the dash over the short duration
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < dashDuration)
        {
            rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, elapsedTime / dashDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPosition);
        Debug.Log("DashEffect: Dashing completed");
    }


    IEnumerator EmitRaycast()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        beamParticleSystem.Play();

        while (elapsedTime < duration)
        {
            RaycastHit hit;
            Vector3 rayOrigin = laserOrigin.position;
            Vector3 rayDirection = laserOrigin.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance, enemyLayer))
            {
                beamParticleSystem.transform.position = hit.point;

                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("EmitRaycast: Enemy hit! It's a kill!");
                    Destroy(hit.collider.gameObject);
                }
            }
            else
            {
                beamParticleSystem.transform.position = rayOrigin + rayDirection * raycastDistance;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        beamParticleSystem.Stop();
        Debug.Log("EmitRaycast: Raycasting ended after " + duration + " seconds");
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
            Debug.Log("ApplyGravity: Applying gravity");
        }
    }

    bool IsGrounded()
    {
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.1f;
        bool grounded = Physics.Raycast(raycastOrigin, Vector3.down, out hit, 0.2f, groundLayer);
        Debug.Log("IsGrounded: " + grounded);
        return grounded;
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

        canGrow = true;
        Debug.Log("ScaleDownAfterDelay: Scaling down complete");
    }

    public void Die()
    {
        Debug.Log("Die: Player died, notifying gameController");
        // Assuming you have a game controller instance handling this
        gameController.instance.PlayerDied(gameObject);
    }

    void OnDestroy()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        if (emitRaycastCoroutine != null)
            StopCoroutine(emitRaycastCoroutine);
        if (dashCoroutine != null)
            StopCoroutine(dashCoroutine);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(laserOrigin.position, laserOrigin.forward * raycastDistance);
    }
}
