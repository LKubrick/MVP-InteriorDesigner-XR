using UnityEngine;

public class MiniatureObjectController : MonoBehaviour
{
    private Vector3 initialPosition;
    public Transform lifeSizeObject; // Assign this in the Inspector

    private void Start()
    {
        // Store the initial position of this object
        initialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Calculate the delta (current position - initial position)
        Vector3 delta = transform.position - initialPosition;

        // Apply the delta to the corresponding life-size object
        lifeSizeObject.position += delta;
    }
}