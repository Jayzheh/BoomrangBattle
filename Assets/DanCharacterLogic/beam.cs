using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class beam : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform handTransform; // Reference to the hand or boomerang
    public float beamLength = 5f; // Length of the beam

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // Line has two points: start and end
    }

    void Update()
    {
        // Update the start position to the hand's position
        lineRenderer.SetPosition(0, handTransform.position);

        // Calculate the end position based on the hand's forward direction and beam length
        Vector3 endPosition = handTransform.position + handTransform.forward * beamLength;
        lineRenderer.SetPosition(1, endPosition);
    }
}
