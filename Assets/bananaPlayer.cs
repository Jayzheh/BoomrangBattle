using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class bananaPlayer : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    Vector2 rotate;
    public float speed = 5f;

    void Awake()
    {
        Debug.Log("Awake called");
        controls = new PlayerControls();

        if (controls != null)
        {
            Debug.Log("Controls initialized successfully.");
            // Add event listeners for input actions
            controls.Gameplay.Grow.performed += ctx => Grow();
            controls.Gameplay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            controls.Gameplay.Move.canceled += ctx => move = Vector2.zero;
            controls.Gameplay.Rotate.performed += ctx => rotate = ctx.ReadValue<Vector2>();
            controls.Gameplay.Rotate.canceled += ctx => rotate = Vector2.zero;
        }
        else
        {
            Debug.LogError("Failed to initialize controls!");
        }
    }

    void Grow()
    {
        transform.localScale *= 1.1f;
    }

    void FixedUpdate()
    {
        Vector2 m = new Vector2(move.x, move.y) * speed * Time.deltaTime;
        transform.Translate(m, Space.World);

        // Example of handling rotation (e.g., rotating the camera or character)
        Vector3 rotation = new Vector3(rotate.y, rotate.x, 0) * speed * Time.deltaTime;
        transform.Rotate(rotation);
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }
}
