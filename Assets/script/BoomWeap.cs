using UnityEngine;
using UnityEngine.InputSystem;

public class BoomWeap : MonoBehaviour
{
    public GameObject boomerangPrefab; // Assign in Unity Editor
    public Transform handPositionTransform; // Assign in Unity Editor
    public float throwForce = 10f; // Adjust as needed
    
    public Animator animator; // Reference to the Animator component

    // Reference to the instantiated boomerang
    GameObject boomerangInstance;

    PlayerControls controls; // Declare PlayerControls variable

    void Start()
    {
        // Initialize PlayerControls
        controls = new PlayerControls();

        // Add event listener for the ThrowBoomerang action
        controls.Gameplay.BoomerangThrow.performed += ctx => ThrowBoomerang();
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

    public void ThrowBoomerang()
    {
        // Set the "Throwing" parameter to true in the Animator
        animator.SetBool("Throwing", true);

        // Ensure there is an existing boomerang instance
        if (boomerangInstance != null)
        {
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
        }
    }

    void Update()
    {
        // Ensure there is an existing boomerang instance
        if (boomerangInstance == null)
        {
            // Instantiate a boomerang object at the hand position
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

        // Update the position of the boomerang to match the hand position
        if (boomerangInstance != null)
        {
            boomerangInstance.transform.position = handPositionTransform.position;
            boomerangInstance.transform.rotation = handPositionTransform.rotation;
        }
    }
}
