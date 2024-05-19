using UnityEngine;

public class MiniatureObjectController : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public Transform lifeSizeObject; // Assign this in the Inspector
    public float multiplyFactor = 10f;

    public Transform[] positions = new Transform[3];
    public Quaternion[] rotations = new Quaternion[3];
    
    private void Start()
    {
        // Store the initial position of this object
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i] == null)
            {
                GameObject newPosHolder = new GameObject("PositionHolder" + i);
                positions[i] = newPosHolder.transform;
                positions[i].position = initialPosition; // Set initial position
            }
        }
    }

    private void FixedUpdate()
    {
        // Calculate the delta (current position - initial position)
        Vector3 delta = transform.position - initialPosition;
        
        
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(initialRotation);
        float deltaY = deltaRot.eulerAngles.y;
        float roundedY = Mathf.Round(deltaY / 45f) * 45f;
        Quaternion deltaRotY = Quaternion.Euler(0, roundedY, 0);
        transform.rotation = initialRotation * deltaRotY;
        
        lifeSizeObject.position += (delta * multiplyFactor);
        lifeSizeObject.rotation = transform.rotation;
        
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void SaveOptions(int index)
    {
        if (index >= 0 && index < 3)
        {
            positions[index].position = transform.position;
            rotations[index] = transform.rotation;
            Debug.Log("position and rotation" + index + "saved");
        }

    }
    
    public void LoadOptions(int index)
    {
        if (index >= 0 && index < 3)
        {
            transform.position = positions[index].position;
            transform.rotation = rotations[index];
            Debug.Log("position and rotation" + index + "loaded");
        }
    }
    
    
}