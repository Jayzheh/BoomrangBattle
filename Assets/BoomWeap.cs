using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoomWeap : MonoBehaviour
{
    [SerializeField] private Transform returnTransform; // Return position of boomerang
    [SerializeField] private float throwForce = 10f; // Throwing force
    [SerializeField] private float returnSpeed = 5f; // Speed at which the boomerang returns

    private Rigidbody rb;
    private bool isReturning = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Boomerang starts as kinematic
    }

    public void Throw()
    {
        Debug.Log("Throwing boomerang...");

        // Release the boomerang
        transform.parent = null;

        // Allow physics to affect the boomerang
        rb.isKinematic = false;
        Debug.Log("isKinematic set to false.");

        // Apply throw force
        rb.velocity = transform.forward * throwForce;
        Debug.Log("Boomerang thrown with velocity: " + rb.velocity);

        // Reset the currentBoomerang reference (if needed)
        // currentBoomerang = null;
    }

    private void Update()
    {
        // Check if the boomerang is returning
        if (isReturning)
        {
            // Move the boomerang towards the return position
            transform.position = Vector3.MoveTowards(transform.position, returnTransform.position, returnSpeed * Time.deltaTime);

            // Check if the boomerang has reached the return position
            if (Vector3.Distance(transform.position, returnTransform.position) < 0.1f)
            {
                // Boomerang has returned, reset its position and velocity
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                transform.position = returnTransform.position;
                rb.isKinematic = true; // Boomerang becomes kinematic again
                isReturning = false;
            }
        }
    }

    public void StartReturning()
    {
        isReturning = true; // Start returning the boomerang
    }
}
