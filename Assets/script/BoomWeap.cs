using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BoomWeap : MonoBehaviour
{
    public GameObject boomerangPrefab; // Assign in Unity Editor
    public Transform handPositionTransform; // Assign in Unity Editor
    public float throwForce = 5f; // Adjust as needed
    public float returnSpeed = 20f; // Increased for faster return
    public float maxThrowDistance = 3f; // Shorter distance for testing

    public Animator animator; // Reference to the Animator component

    // Reference to the instantiated boomerang
    private GameObject boomerangInstance;

    private PlayerControls controls; // Declare PlayerControls variable

    public bool isBoomerangThrown = false;
    private Vector3 startPosition;

    void Awake()
    {
        // Initialize PlayerControls
        controls = new PlayerControls();

        // Add event listener for the ThrowBoomerang action
        controls.Gameplay.BoomerangThrow.performed += ctx => TriggerBoomerangThrow();
    }

    void OnEnable()
    {
        // Ensure controls object is not null before enabling
        if (controls == null)
        {
            controls = new PlayerControls();
        }

        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        // Ensure controls object is not null before disabling
        if (controls != null)
        {
            controls.Gameplay.Disable();
        }
    }

    private void TriggerBoomerangThrow()
    {
        // Call the ThrowBoomerang method
        if (!isBoomerangThrown)
        {
            StartCoroutine(ThrowBoomerang());
        }
    }

    public IEnumerator ThrowBoomerang()
    {
        // Ensure there is an existing boomerang instance and it's not already thrown
        if (boomerangInstance != null && !isBoomerangThrown)
        {
            Debug.Log("Throwing boomerang");

            // Set the "BoomerangThrow" trigger in the Animator
            animator.SetTrigger("BoomerangThrow");

            // Wait for a short period to sync with the animation
            yield return new WaitForSeconds(0.2f); // Adjust the duration as needed

            // Set the "Throwing" parameter to true in the Animator
            animator.SetBool("Throwing", true);

            // Get the Rigidbody component from the instantiated object
            Rigidbody rb = boomerangInstance.GetComponent<Rigidbody>();

            // Check if the instantiated object has a Rigidbody component
            if (rb != null)
            {
                // Calculate the direction the boomerang should be thrown
                Vector3 throwDirection = handPositionTransform.forward;

                // Set isKinematic to false to enable physics
                rb.isKinematic = false;

                // Apply force to the boomerang
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

                // Set the start position for the return calculation
                startPosition = boomerangInstance.transform.position;
            }

            // Detach the boomerang from the player's hand
            boomerangInstance.transform.parent = null;

            // Set the flag to true to prevent further throwing until a new boomerang is instantiated
            isBoomerangThrown = true;

            // Reset the "Throwing" parameter to false after a short delay
            StartCoroutine(ResetThrowingParameter());

            // Wait for the boomerang to reach its max distance and then return
            yield return new WaitUntil(() => Vector3.Distance(startPosition, boomerangInstance.transform.position) >= maxThrowDistance);

            StartCoroutine(ReturnBoomerang());
        }
    }

    private IEnumerator ReturnBoomerang()
    {
        Debug.Log("Boomerang returning");

        Rigidbody rb = boomerangInstance.GetComponent<Rigidbody>();

        // Ensure the boomerang has a Rigidbody component
        if (rb != null)
        {
            // Set the boomerang to kinematic for smooth return
            rb.isKinematic = true;

            // Calculate the return direction
            Vector3 returnDirection = (handPositionTransform.position - boomerangInstance.transform.position).normalized;

            while (Vector3.Distance(boomerangInstance.transform.position, handPositionTransform.position) > 0.1f)
            {
                // Move the boomerang towards the hand position
                boomerangInstance.transform.position = Vector3.MoveTowards(boomerangInstance.transform.position, handPositionTransform.position, returnSpeed * Time.deltaTime);
                yield return null;
            }

            // Reattach the boomerang to the hand
            boomerangInstance.transform.position = handPositionTransform.position;
            boomerangInstance.transform.rotation = handPositionTransform.rotation;
            boomerangInstance.transform.parent = handPositionTransform;

            // Debug message for when the boomerang returns
            Debug.Log("Boomerang has returned to the player's hand.");

            // Reset isBoomerangThrown to false
            isBoomerangThrown = false;
        }
    }

    private IEnumerator ResetThrowingParameter()
    {
        // Wait for a short period before resetting the "Throwing" parameter
        yield return new WaitForSeconds(2f); // Adjust the duration to match the animation length

        // Reset the "Throwing" parameter to false in the Animator
        animator.SetBool("Throwing", false);
    }

    void Update()
    {
        // Ensure the boomerang stays in the hand position until it is thrown
        if (boomerangInstance != null && !isBoomerangThrown)
        {
            Rigidbody rb = boomerangInstance.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.isKinematic = true;
            }

            boomerangInstance.transform.position = handPositionTransform.position;
            boomerangInstance.transform.rotation = handPositionTransform.rotation;
        }
    }
}
