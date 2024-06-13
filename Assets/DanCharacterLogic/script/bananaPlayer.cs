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
    private Animator animator;
    private Rigidbody rb; // Rigidbody component reference

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float growthDuration = 2f;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private ParticleSystem beamParticleSystem;
    [SerializeField] private LayerMask enemyLayer; // Layer for the enemy

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
        };

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component from the same GameObject
        rb.freezeRotation = true; // Freeze rotation to prevent unwanted physics behavior
        rb.useGravity = false; // Disable gravity for now, as we handle it manually
    }

    void Start()
    {
        originalMovementSpeed = movementSpeed;
        Debug.Log("Start: Initialization complete");
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
        Debug.Log("OnEnable: Controls enabled");
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
        Debug.Log("OnDisable: Controls disabled");
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
        rb.MovePosition(transform.position + transform.TransformDirection(movement)); // Use Rigidbody.MovePosition for physics-based movement

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

        // Play Particle System
        beamParticleSystem.Play();

        while (elapsedTime < duration)
        {
            RaycastHit hit;
            Vector3 rayOrigin = laserOrigin.position;
            Vector3 rayDirection = laserOrigin.forward;

            // Perform raycast
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance, enemyLayer))
            {
                // Move Particle System to hit point
                beamParticleSystem.transform.position = hit.point;

                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("EmitRaycast: Enemy hit! It's a kill!");
                    Destroy(hit.collider.gameObject); // Destroy the enemy
                }
            }
            else
            {
                // Move Particle System to max distance
                beamParticleSystem.transform.position = rayOrigin + rayDirection * raycastDistance;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop Particle System when raycasting ends
        beamParticleSystem.Stop();
        Debug.Log("EmitRaycast: Raycasting ended after " + duration + " seconds");
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration); // Use Rigidbody.AddForce for gravity
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
        float elapsedTime = 0f;
        float dashDistance = 2f; // Distance to dash

        // Calculate the destination position
        Vector3 destination = transform.position + transform.forward * dashDistance;

        // Move the player to the destination position with physics
        while (elapsedTime < dashDuration)
        {
            // Calculate the distance to move this frame: speed * deltaTime
            float step = dashSpeedBoost * Time.deltaTime;

            // Move towards the destination
            rb.MovePosition(Vector3.MoveTowards(transform.position, destination, step));

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Check if we've reached the destination early
            if (Vector3.Distance(transform.position, destination) < 0.1f)
                break;

            yield return null;
        }

        // Ensure the player's position is exactly at the destination
        rb.MovePosition(destination);

        // Wait for a short duration at the destination
        yield return new WaitForSeconds(0.1f); // Adjust this duration as needed

        // Reset any dash-related state or effects here
        // For example, you might reset velocity or cooldowns

        Debug.Log("DashEffect: Dashing completed");
    }

    void Die()
    {
        Debug.Log("Die: Player died, notifying gameController");
        gameController.instance.PlayerDied(gameObject);
    }
}
