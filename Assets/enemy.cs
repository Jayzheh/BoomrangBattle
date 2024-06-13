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

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float minDistanceToOtherEnemies = 5f; // Increased minimum distance to maintain between enemies
    [SerializeField] private float separationWeight = 5f; // Strength of the separation behavior

    private Rigidbody rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the Rigidbody from rotating
        rb.useGravity = false; // We handle gravity manually
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);

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

        // Check distance to other enemies and adjust position if necessary
        AvoidOtherEnemies();
    }

    void AttackPlayer()
    {
        if (Time.time > attackCooldownTimer)
        {
            int randomAttack = Random.Range(0, 3); // Randomly select an attack

            switch (randomAttack)
            {
                case 0:
                    Slash();
                    break;
                case 1:
                    BoomerangThrow();
                    break;
                case 2:
                    Dash();
                    break;
            }

            attackCooldownTimer = Time.time + attackCooldown;
        }
    }

    void Slash()
    {
        animator.SetTrigger("Slash");

        // Perform invisible raycast
        PerformInvisibleRaycast();
    }

    void PerformInvisibleRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange, playerLayer))
        {
            Debug.Log($"PerformInvisibleRaycast: Player hit by invisible raycast");

            // Check if the player has a component of type 'bananaPlayer'
            bananaPlayer player = hit.collider.GetComponent<bananaPlayer>();
            if (player != null)
            {
                // Call the Die method if accessible
                player.Die();
            }
        }
    }

    void BoomerangThrow()
    {
        animator.SetTrigger("BoomerangThrow");

        // Perform raycast to check if player is hit by the boomerang throw
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (playerTransform.position - transform.position).normalized, out hit, attackRange, playerLayer))
        {
            Debug.Log("BoomerangThrow: Player hit by boomerang throw");
            bananaPlayer player = hit.collider.GetComponent<bananaPlayer>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    void Dash()
    {
        animator.SetTrigger("Dash");

        // Perform dash towards the player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        rb.AddForce(direction * 10f, ForceMode.Impulse);
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
    }

    void ApplyGravity()
    {
        // Simulate gravity if not grounded
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration); // Use Rigidbody.AddForce for gravity
        }
    }

    bool IsGrounded()
    {
        // Check if the enemy is grounded using raycast
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.1f;
        bool grounded = Physics.Raycast(raycastOrigin, Vector3.down, out hit, 0.2f, groundLayer);
        return grounded;
    }

    void Die()
    {
        // Handle enemy death, notify game controller
        Debug.Log("Die: Enemy died, notifying gameController");
        gameController.instance.BotDied(gameObject);
    }

    void AvoidOtherEnemies()
    {
        // Find all enemy objects in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy != gameObject) // Avoid checking against self
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy < minDistanceToOtherEnemies)
                {
                    // Calculate the direction away from the other enemy
                    Vector3 directionAway = (transform.position - enemy.transform.position).normalized;
                    Vector3 separationForce = directionAway * separationWeight / distanceToEnemy;

                    // Apply the separation force
                    rb.AddForce(separationForce, ForceMode.Acceleration);
                }
            }
        }
    }
}
