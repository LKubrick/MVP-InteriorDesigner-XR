using System;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class MiniatureObjectController : MonoBehaviour
{
    private Vector3 initialPosition;
    public Dollhouse _dollhouse;
    public Transform lifeSizeObject; // Assign this in the Inspector
    public float multiplyFactor = 8f;

    private bool isBeingGrabbed = false;
    
    private void Start()
    {
        // Store the initial position of this object
        initialPosition = transform.localPosition;
    }

    // checks if I (mini object) am being grabbed right now
    private bool IsBeingGrabbed()
    {
        var grabInteractable = gameObject.GetComponentInChildren<HandGrabInteractable>();
        if (grabInteractable != null)
        {
            return grabInteractable.State == InteractableState.Select;
        }
        return false;
    }
    
    private void FixedUpdate()
    {
        Vector3 delta = transform.localPosition - initialPosition;
        initialPosition = transform.localPosition;
        
        // trap the release of an object
        if (isBeingGrabbed && !IsBeingGrabbed())
        {
            if (_dollhouse.IsInLineup(gameObject))
            {
                _dollhouse.AddToScene(gameObject);
                return;
            }
            else
            {
                // XXX check outside dollhouse bounds
                if (gameObject.transform.position.y < 1.0f)
                {
//                    _dollhouse.AddToLineup(gameObject);
  //                  return;
                } 
                
                //snap to floor
    //            var initialPos = _dollhouse.GetInitialPosition(gameObject);
     //           var newPos = new Vector3(transform.localPosition.x, initialPos.y, transform.localPosition.z);
       //         transform.localPosition = newPos;
            }
        }
        isBeingGrabbed = IsBeingGrabbed();
        
        // Keep object horizontal XXX
        //float xRotation = transform.localRotation.eulerAngles.x;
        //float zRotation = transform.localRotation.eulerAngles.z;
        //Quaternion yRotationQtrn = Quaternion.Euler(xRotation, 0, zRotation);
        //transform.rotation = yRotationQtrn;
        
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