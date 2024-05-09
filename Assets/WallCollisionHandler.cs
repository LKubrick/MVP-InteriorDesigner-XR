using UnityEngine;

public class WallCollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Stop the life-size object's movement by setting its velocity to zero
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}