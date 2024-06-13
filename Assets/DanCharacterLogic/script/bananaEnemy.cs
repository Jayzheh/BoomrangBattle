using UnityEngine;
using UnityEngine.AI;

public class bananaEnemy : MonoBehaviour
{
    public float movementSpeed = 3.5f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private float attackCooldownTimer;
    public float detectionRange = 10f;
    private Transform playerTransform;
    private Animator animator;
    private NavMeshAgent navAgent;

    private Vector3 originalPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = movementSpeed;

        originalPosition = transform.position;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
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
    }

    void AttackPlayer()
    {
        navAgent.isStopped = true;
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
        navAgent.isStopped = false;
        navAgent.SetDestination(playerTransform.position);

        animator.SetFloat("Speed", navAgent.velocity.magnitude);
        animator.SetBool("IsMoving", true);
        Debug.Log("PursuePlayer: Pursuing the player");
    }

    void Patrol()
    {
        if (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(originalPosition);

            animator.SetFloat("Speed", navAgent.velocity.magnitude);
            animator.SetBool("IsMoving", true);
            Debug.Log("Patrol: Patrolling");
        }
        else
        {
            navAgent.isStopped = true;
            animator.SetFloat("Speed", 0);
            animator.SetBool("IsMoving", false);
            Debug.Log("Patrol: Standing idle");
        }
    }
}
