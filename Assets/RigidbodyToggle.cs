using UnityEngine;

public class RigidbodyToggle : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("No Rigidbody component found on this GameObject.");
        }
    }

    // Method to enable the Rigidbody
    public void EnableRigidbody()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }

    // Method to disable the Rigidbody
    public void DisableRigidbody()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    // Method to toggle the Rigidbody
    public void ToggleRigidbody()
    {
        if (rb != null)
        {
            if (rb.isKinematic)
            {
                EnableRigidbody();
            }
            else
            {
                DisableRigidbody();
            }
        }
    }
}