using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private float attackCooldownTimer;
    public float detectionRange = 10f;
    private Transform playerTransform;
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    public float laserAttackIntervalMin = 2f;
    public float laserAttackIntervalMax = 5f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private LineRenderer lineRenderer;

    void Awake()
    {
        Debug.Log("Awake: Initializing enemy components");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Set up the LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Start()
    {
        Debug.Log("Start: Finding player and starting RandomLaserAttack coroutine");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(RandomLaserAttack());
    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        Debug.Log("FixedUpdate: Distance to player: " + distanceToPlayer);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
            else
            {
                PursuePlayer();
            }
        }
        else
        {
            Patrol();
        }

        ApplyGravity();
        AdjustColliderHeight();
        Debug.Log("FixedUpdate: Grounded: " + IsGrounded());
    }

    void AttackPlayer()
    {
        if (Time.time > attackCooldownTimer)
        {
            animator.SetTrigger("Slash");
            Debug.Log("AttackPlayer: Attacking the player");

            // Implement your damage logic here

            attackCooldownTimer = Time.time + attackCooldown;
        }
    }

    void PursuePlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Ensure the enemy does not tilt up or down
        Vector3 movement = direction * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Rotate to face the player
        Quaternion toRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 5f);

        animator.SetFloat("Speed", 1);
        animator.SetBool("IsMoving", true);
        Debug.Log("PursuePlayer: Pursuing the player");
    }

    void Patrol()
    {
        if (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            Vector3 direction = (originalPosition - transform.position).normalized;
            direction.y = 0; // Ensure the enemy does not tilt up or down
            Vector3 movement = direction * movementSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);

            // Rotate to face the original direction
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 2);
        }

        animator.SetFloat("Speed", 0.5f);
        animator.SetBool("IsMoving", true);
        Debug.Log("Patrol: Patrolling");
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
        capsuleCollider.height = 2.02f;
        Debug.Log("AdjustColliderHeight: Adjusted height to " + capsuleCollider.height);
    }

    IEnumerator RandomLaserAttack()
    {
        while (true)
        {
            float waitTime = Random.Range(laserAttackIntervalMin, laserAttackIntervalMax);
            Debug.Log("RandomLaserAttack: Waiting for " + waitTime + " seconds before next attack");
            yield return new WaitForSeconds(waitTime);

            if (Vector3.Distance(playerTransform.position, transform.position) <= detectionRange)
            {
                StartCoroutine(EmitRaycast());
            }
        }
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
                Debug.Log("EmitRaycast: Ray hit " + hit.collider.name);

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
                Debug.Log("EmitRaycast: Ray did not hit anything");
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable LineRenderer when raycasting ends
        lineRenderer.enabled = false;
        Debug.Log("EmitRaycast: Raycasting ended after " + duration + " seconds");
    }

    void Die()
    {
        Debug.Log("Die: Enemy died, notifying gameController");
        gameController.instance.BotDied(gameObject);
    }
}
