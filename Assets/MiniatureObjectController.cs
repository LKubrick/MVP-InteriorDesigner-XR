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
    public float initialYLocalPosLifesizeObject; // this is a hack -- was getting placed too high upon user release
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

    Bounds CalculateBoundingBox(GameObject obj)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; ++i)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
    
    private void FixedUpdate()
    {
        Vector3 delta = transform.localPosition - initialPosition;
        initialPosition = transform.localPosition;

        // trap the release of an object
        if (isBeingGrabbed && !IsBeingGrabbed())
        {
            isBeingGrabbed = IsBeingGrabbed();

            if (_dollhouse.IsInLineup(gameObject))
            {
                _dollhouse.AddToScene(gameObject);
                return;
            }
            else
            {
                // check if outside dollhouse bounds - project onto 2d x-z plane of floor
                bool isBelowFloor = transform.position.y < _dollhouse._floor.transform.position.y;
                if (isBelowFloor)
                {
                    _dollhouse.AddToLineup(gameObject);
                    return;
                }

                //snap to floor
                var initialPos = _dollhouse.GetInitialPosition(gameObject);
                var newPos = new Vector3(transform.localPosition.x, initialPos.y, transform.localPosition.z);
                float xRotation = -90f; // empirically, what seems to work in our scene
                float yRotation = transform.localRotation.eulerAngles.y;
                float zRotation = transform.localRotation.eulerAngles.z;
                Quaternion rotationQtrn = Quaternion.Euler(xRotation, yRotation, zRotation);
                transform.localRotation = rotationQtrn;
                transform.localPosition = newPos;
            }
        }
        else
        {
            isBeingGrabbed = IsBeingGrabbed();
        }
        // Apply the delta to the corresponding life-size object
        if (lifeSizeObject)
        {
            lifeSizeObject.localPosition += delta * multiplyFactor;
            lifeSizeObject.localRotation = transform.localRotation;
            var p = lifeSizeObject.localPosition;
            lifeSizeObject.localPosition = new Vector3(p.x, initialYLocalPosLifesizeObject, p.z);
        }
    }
}