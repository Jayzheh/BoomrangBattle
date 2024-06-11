using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BoomWeap : MonoBehaviour
{
    public GameObject boomerangPrefab; // Assign in Unity Editor
    public Transform handPositionTransform; // Assign in Unity Editor
    public float throwForce = 10f; // Adjust as needed

    public Animator animator; // Reference to the Animator component

    // Reference to the instantiated boomerang
    private GameObject boomerangInstance;

    private PlayerControls controls; // Declare PlayerControls variable

    public bool isBoomerangThrown = false;

    void Awake()
    {
        // Initialize PlayerControls
        controls = new PlayerControls();

        // Add event listener for the ThrowBoomerang action
        controls.Gameplay.BoomerangThrow.performed += ctx => TriggerBoomerangThrow();
    }

    void Start()
    {
        // Instantiate the boomerang at the start and set it to kinematic
        InstantiateBoomerang();
    }

    void OnEnable()
    {
        // Ensure controls object is not null before enabling
        if (controls != null)
        {
            // Enable the PlayerControls
            controls.Gameplay.Enable();
        }
    }

    void OnDisable()
    {
        // Ensure controls object is not null before disabling
        if (controls != null)
        {
            // Disable the PlayerControls
            controls.Gameplay.Disable();
        }
    }

    private void InstantiateBoomerang()
    {
        // Instantiate the boomerang if it doesn't already exist
        if (boomerangInstance == null)
        {
            Debug.Log("Instantiating boomerang in hand");
            boomerangInstance = Instantiate(boomerangPrefab, handPositionTransform.position, handPositionTransform.rotation);

            // Get the Rigidbody component from the instantiated object
            Rigidbody rb = boomerangInstance.GetComponent<Rigidbody>();

            // Check if the instantiated object has a Rigidbody component
            if (rb != null)
            {
                // Set isKinematic to true to prevent physics from affecting the boomerang
                rb.isKinematic = true;
            }
        }
    }

    private void TriggerBoomerangThrow()
    {
        // Call the ThrowBoomerang method
        StartCoroutine(ThrowBoomerang());
    }

    public IEnumerator ThrowBoomerang() // Changed from private to public
    {
        // Ensure there is an existing boomerang instance and it's not already thrown
        if (boomerangInstance != null && !isBoomerangThrown)
        {
            Debug.Log("Throwing boomerang");

            // Set the "BoomerangThrow" trigger in the Animator
            animator.SetTrigger("BoomerangThrow");

            // Wait for a short period to sync with the animation
            yield return new WaitForSeconds(0.1f); // Adjust the duration as needed

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
            }

            // Set the flag to true to prevent further throwing until a new boomerang is instantiated
            isBoomerangThrown = true;

            // Reset the "Throwing" parameter to false after a short delay
            StartCoroutine(ResetThrowingParameter());
        }
    }

    private IEnumerator ResetThrowingParameter()
    {
        // Wait for a short period before resetting the "Throwing" parameter
        yield return new WaitForSeconds(0.5f); // Adjust the duration as needed

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
