using UnityEngine;
using System.Collections;

public class bananaEnemy : MonoBehaviour
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
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private LineRenderer lineRenderer;

    void Awake()
    {
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
        // Simple patrol logic: move back to original position if far away
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
            // Rotate to face original direction
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
}
