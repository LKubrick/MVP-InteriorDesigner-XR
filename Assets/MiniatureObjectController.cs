using UnityEngine;

public class MiniatureObjectController : MonoBehaviour
{
    private Vector3 initialPosition;
    public Transform lifeSizeObject; // Assign this in the Inspector
    public float multiplyFactor = 8f;

    private void Start()
    {
        // Store the initial position of this object
        initialPosition = transform.localPosition;
    }

    private void FixedUpdate()
    {
        // Calculate the delta (current position - initial position)
        Vector3 delta = transform.localPosition - initialPosition;
        initialPosition = transform.localPosition;

        if (delta.magnitude > 0.001f)
        {
            // Apply the delta to the corresponding life-size object
            if (lifeSizeObject)
            {
                lifeSizeObject.localPosition += (delta * multiplyFactor);
                lifeSizeObject.localRotation = transform.localRotation;
            }

        }
    }
}