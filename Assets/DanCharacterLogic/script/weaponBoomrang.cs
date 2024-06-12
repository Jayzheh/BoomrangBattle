using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class weaponBoomrang : MonoBehaviour
{
     public enum Interpolation
    {
        None,
        Linear,
        Radial
    }

    public enum Constraints
    {
        None,
        FixedX,
        FixedY,
        FixedZ,
    }

    [Tooltip("Speed of the boomerang movement")]
    public float speed = 50f;
    [Tooltip("Amount of the boomerang rotation per second")]
    public Vector3 torque = new Vector3(0, 1440, 0);

    [Tooltip("Throw position of boomerang")]
    public Transform throwTransform;

    [Tooltip("Return position of boomerang (keep null to use Throw Transform)")]
    public Transform returnTransform;

    [Tooltip("Interpolation method for return direction")]
    public Interpolation interpolation = Interpolation.Linear;

    [Tooltip("Time to follow the target transform")]
    public float navigatingDuration = 2;

    [Tooltip("Curve for interpolation")]
    public AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Constraints for boomerang movement")]
    public Constraints constraints = Constraints.FixedY;

    [Tooltip("Distance to skip applying constraints")]
    public float constraintsSkipDistance = 5f;

    [Tooltip("Radius for collisions")]
    public float radius = 0.3f;

    [Tooltip("Distance to be considered as returned")]
    public float returnedDistance = 0.5f;

    [Tooltip("Force applied to colliding rigid bodies")]
    public float pushForce = 0.2f;

    [Tooltip("Return when hitting something")]
    public bool returnOnHit = false;

    [Header("Decorations")]
    public TrailRenderer[] trails;
    public ParticleSystem[] particles;

    [Tooltip("Called when weapon has been thrown")]
    public UnityEvent OnThrow;

    [Tooltip("Called when weapon has returned")]
    public UnityEvent OnReturn;

    private bool isReturning;
    private bool isThrown;
    private Vector3 direction;
    private Transform oldParent;
    private Vector3 oldLocalPos;
    private Quaternion oldLocalRot;
    private Vector3 dirScale;
    private List<Collider> playerColliders = new List<Collider>();
    private RaycastHit[] hitBuffer = new RaycastHit[20];
    private float attractionTime;
    private Transform targetTransform;

    private void Start()
    {
        oldParent = transform.parent;
        oldLocalPos = transform.localPosition;
        oldLocalRot = transform.localRotation;
        
        switch (constraints)
        {
            case Constraints.FixedX:
                dirScale = new Vector3(0, 1, 1);
                break;
            case Constraints.FixedY:
                dirScale = new Vector3(1, 0, 1);
                break;
            case Constraints.FixedZ:
                dirScale = new Vector3(1, 1, 0);
                break;
            default:
                dirScale = Vector3.one;
                break;
        }
    }

    private void Update()
    {
        if (!isThrown) return;

        if (isReturning)
        {
            if (targetTransform != returnTransform)
            {
                targetTransform = returnTransform;
                attractionTime = 0;
            }

            var diff = returnTransform.position - transform.position;
            diff.Scale(dirScale);
            if (diff.magnitude < returnedDistance)
            {
                ResetWeapon();
                return;
            }
        }

        // Handle collisions
        int count = Physics.SphereCastNonAlloc(transform.position, radius, direction, hitBuffer, speed * Time.deltaTime);
        for (int i = 0; i < count; i++)
        {
            // Handle collision logic here (e.g., reflect direction, apply force, etc.)
            if (returnOnHit)
            {
                isReturning = true;
            }
        }

        // Move the boomerang
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(torque * Time.deltaTime);

        // Interpolation logic
        attractionTime += Time.deltaTime;
        float interpolationFactor = interpolationCurve.Evaluate(attractionTime / navigatingDuration);
        Vector3 straightDirection = (targetTransform.position - transform.position).normalized;
        direction = Vector3.Slerp(direction, straightDirection, interpolationFactor).normalized;
    }

    public void Throw()
    {
        if (isThrown) return;

        if (throwTransform != null)
        {
            transform.position = throwTransform.position;
            transform.rotation = throwTransform.rotation;
        }

        targetTransform = returnTransform != null ? returnTransform : throwTransform;
        direction = targetTransform.forward;
        isThrown = true;
        isReturning = false;

        OnThrow?.Invoke();
    }

    public void ResetWeapon()
    {
        isThrown = false;
        isReturning = false;
        transform.parent = oldParent;
        transform.localPosition = oldLocalPos;
        transform.localRotation = oldLocalRot;
        foreach (var trail in trails)
        {
            trail.Clear();
        }
        foreach (var particle in particles)
        {
            particle.Clear();
        }
        OnReturn?.Invoke();
    }
}
