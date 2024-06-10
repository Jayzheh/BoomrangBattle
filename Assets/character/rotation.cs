using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorRotation : MonoBehaviour
{
    // Speed of rotation in degrees per second
    public float rotationSpeed = 45f;

    // Update is called once per frame
    void Update()
    {
        // Rotate the Props-3 GameObject around its Y-axis at the specified speed
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
