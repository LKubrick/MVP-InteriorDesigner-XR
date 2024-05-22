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
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void FixedUpdate()
    {
        // Calculate the delta (current position - initial position)
        Vector3 delta = transform.localPosition - initialPosition;
        Quaternion deltaRot = transform.localRotation * Quaternion.Inverse(initialRotation);
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        // XXX check for rotation too
        if (delta.magnitude > 0.001f)
        {
            // Apply the delta to the corresponding life-size object
            if (lifeSizeObject)
            {
                lifeSizeObject.localPosition += (delta * multiplyFactor);
                lifeSizeObject.localRotation = initialRotation * deltaRot;
            }

        }
    }
}