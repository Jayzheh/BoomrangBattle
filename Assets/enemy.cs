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
    private Animator animator;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    public float laserAttackIntervalMin = 2f;
    public float laserAttackIntervalMax = 5f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private ParticleSystem beamParticleSystem;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody rb;

    void Awake()
    {
        Debug.Log("Awake: Initializing enemy components");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the Rigidbody from rotating
        rb.useGravity = false; // We handle gravity manually
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Start()
    {
        Debug.Log("Start: Finding player and starting RandomLaserAttack coroutine");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        beamParticleSystem.Stop(); // Ensure Particle System starts off
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
        Debug.Log("FixedUpdate: Grounded: " + IsGrounded());
    }

    void AttackPlayer()
    {
        if (Time.time > attackCooldownTimer)
        {
            animator.SetTrigger("Slash");
            Debug.Log("AttackPlayer: Attacking the player");

            // Perform the invisible ray beam attack
            PerformRayBeamAttack();

            attackCooldownTimer = Time.time + attackCooldown;
        }
    }

    void PerformRayBeamAttack()
    {
        Ray ray = new Ray(laserOrigin.position, laserOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionRange, playerLayer))
        {
            Debug.Log("PerformRayBeamAttack: Player hit by invisible ray beam");

            // Assuming the player has a method "Die" to handle death
            Player player = hit.collider.GetComponent<Player>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    void PursuePlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Ensure the enemy does not tilt up or down
        Vector3 movement = direction * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement); // Use Rigidbody for movement

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
            rb.MovePosition(transform.position + movement); // Use Rigidbody for movement

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
        // Simulate gravity if not grounded
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration); // Use Rigidbody.AddForce for gravity
            Debug.Log("ApplyGravity: Applying gravity");
        }
        else
        {
            Debug.Log("ApplyGravity: Grounded, resetting vertical position");
        }
    }

    bool IsGrounded()
    {
        // Check if the enemy is grounded using raycast
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.1f;
        bool grounded = Physics.Raycast(raycastOrigin, Vector3.down, out hit, 0.2f, groundLayer);
        Debug.Log("IsGrounded: " + grounded);
        return grounded;
    }

    IEnumerator RandomLaserAttack()
    {
        // Continuously perform laser attacks randomly
        while (true)
        {
            float waitTime = Random.Range(laserAttackIntervalMin, laserAttackIntervalMax);
            Debug.Log("RandomLaserAttack: Waiting for " + waitTime + " seconds before next attack");
            yield return new WaitForSeconds(waitTime);

            if (Vector3.Distance(playerTransform.position, transform.position) <= detectionRange)
            {
                StartBeamEffect();
                yield return new WaitForSeconds(1f); // Adjust beam duration if needed
                StopBeamEffect();
            }
        }
    }

    void StartBeamEffect()
    {
        // Start the particle beam effect
        beamParticleSystem.Play();
        Debug.Log("StartBeamEffect: Started beam effect");
    }

    void StopBeamEffect()
    {
        // Stop the particle beam effect
        beamParticleSystem.Stop();
        Debug.Log("StopBeamEffect: Stopped beam effect");
    }

    void Die()
    {
        // Handle enemy death, notify game controller
        Debug.Log("Die: Enemy died, notifying gameController");
        gameController.instance.BotDied(gameObject);
    }
}
