using UnityEngine;

public class MiniatureObjectController : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public Transform lifeSizeObject; // Assign this in the Inspector
    public float multiplyFactor = 8f;

    private void Start()
    {
        // Store the initial position of this object
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        // Calculate the delta (current position - initial position)
        Vector3 delta = transform.position - initialPosition;
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(initialRotation);

        // Apply the delta to the corresponding life-size object
        lifeSizeObject.position += (delta * multiplyFactor);
        initialPosition = transform.position;
        lifeSizeObject.rotation = initialRotation * deltaRot;
        initialRotation = transform.rotation;
    }
}