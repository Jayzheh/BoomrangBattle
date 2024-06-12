using UnityEngine;

namespace Hykudoru
{
    public enum Plane
    {
        Undefined = 0,
        XY,
        XZ,
        YZ
    }

    public class Boomerang : MonoBehaviour
    {
        private Transform characterTransform; // Reference to the character's transform
        private Vector3 initialPosition;
        private Vector3 deltaPosition = Vector3.zero;

        [SerializeField] private bool halfCycle = false;
        public bool HalfCycle { get { return halfCycle; } set { halfCycle = value; } }

        [SerializeField] private bool localSpace = false;
        public bool LocalSpace { get { return localSpace; } set { localSpace = value; } }

        [SerializeField] private Plane orbitalPlane = Plane.Undefined;
        public Plane OrbitalPlane { get { return orbitalPlane; } set { orbitalPlane = value; } }

        [SerializeField] private Vector3 velocity = Vector3.zero;
        public Vector3 Velocity { get { return velocity; } set { velocity = value; } }

        [SerializeField] private Vector3 displacement = Vector3.one;
        public Vector3 Displacement { get { return displacement; } set { displacement = value; } }

        [SerializeField] [Range(0f, 10f)]
        private float frequencyMultiplier = 1f;
        public float FrequencyMultiplier { get { return frequencyMultiplier; } set { frequencyMultiplier = Mathf.Clamp(value, 0f, 10f); } }

        [SerializeField] [Range(0f, 1000f)]
        private float amplitudeMultiplier = 1f;
        public float AmplitudeMultiplier { get { return amplitudeMultiplier; } set { amplitudeMultiplier = Mathf.Clamp(value, 0f, 1000f); } }

        private void Start()
        {
            // Get a reference to the character's transform
            characterTransform = GameObject.FindWithTag("Banana").transform;

            initialPosition = transform.position;
        }

        private void LateUpdate()
        {
            // Update the boomerang's position relative to the character
            transform.position = characterTransform.position;
        }

        private void Oscillate()
        {
            float xSpeed = (Mathf.PI * 2 * Velocity.x * Time.timeSinceLevelLoad) * FrequencyMultiplier;
            float ySpeed = (Mathf.PI * 2 * Velocity.y * Time.timeSinceLevelLoad) * FrequencyMultiplier;
            float zSpeed = (Mathf.PI * 2 * Velocity.z * Time.timeSinceLevelLoad) * FrequencyMultiplier;
            float deltaX = Displacement.x * AmplitudeMultiplier;
            float deltaY = Displacement.y * AmplitudeMultiplier;
            float deltaZ = Displacement.z * AmplitudeMultiplier;

            switch (orbitalPlane)
            {
                case Plane.XY:
                    deltaX *= Mathf.Cos(xSpeed);
                    deltaY *= Mathf.Sin(ySpeed);
                    if (halfCycle)
                    {
                        deltaY = deltaY < 0 ? Mathf.Abs(deltaY) : deltaY;
                    }
                    break;

                case Plane.XZ:
                    deltaX *= Mathf.Cos(xSpeed);
                    deltaZ *= Mathf.Sin(zSpeed);
                    if (halfCycle)
                    {
                        deltaZ = deltaZ < 0 ? Mathf.Abs(deltaZ) : deltaZ;
                    }
                    break;

                case Plane.YZ:
                    deltaZ *= Mathf.Cos(zSpeed);
                    deltaY *= Mathf.Sin(ySpeed);
                    if (halfCycle)
                    {
                        deltaY = deltaY < 0 ? Mathf.Abs(deltaY) : deltaY;
                    }
                    break;

                default:
                    deltaX *= Mathf.Sin(xSpeed);
                    deltaY *= Mathf.Sin(ySpeed);
                    deltaZ *= Mathf.Sin(zSpeed);
                    if (halfCycle)
                    {
                        deltaX = deltaX < 0 ? Mathf.Abs(deltaX) : deltaX;
                        deltaY = deltaY < 0 ? Mathf.Abs(deltaY) : deltaY;
                        deltaZ = deltaZ < 0 ? Mathf.Abs(deltaZ) : deltaZ;
                    }
                    break;
            }

            if (localSpace)
            {
                deltaPosition = (transform.right * deltaX) + (transform.up * deltaY) + (transform.forward * deltaZ);
            }
            else
            {
                deltaPosition.Set(deltaX, deltaY, deltaZ);
            }

            // Update the boomerang's position relative to the character
            transform.position = characterTransform.position + deltaPosition;
        }

        public void ResetPosition()
        {
            transform.position = initialPosition;
            deltaPosition = Vector3.zero;
        }
    }
}
