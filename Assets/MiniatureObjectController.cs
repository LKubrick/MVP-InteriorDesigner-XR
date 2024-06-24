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
    public double initialYLocalPosLifesizeObject; // this is a hack -- was getting placed too high upon user release
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

    void MakeHorizontal()
    {
        float xRotation = -90f; // empirically, what seems to work in our scene
        float yRotation = transform.localRotation.eulerAngles.y;
        float zRotation = transform.localRotation.eulerAngles.z;
        Quaternion rotationQtrn = Quaternion.Euler(xRotation, yRotation, zRotation);
        transform.localRotation = rotationQtrn;
    }
    private void FixedUpdate()
    {
        Vector3 delta = transform.localPosition - initialPosition;
        initialPosition = transform.localPosition;

        // trap the release of an object
        var wasBeingGrabbed = isBeingGrabbed; // was being grabbed in previous update call?
        isBeingGrabbed = IsBeingGrabbed(); // update variable

        if (!wasBeingGrabbed && IsBeingGrabbed())
        {
            // item was just pinched
            if (_dollhouse.IsInLineup(gameObject))
            {
                MakeHorizontal();
                _dollhouse.AddToScene(gameObject);
                return;
            }
        }
        if (wasBeingGrabbed && !IsBeingGrabbed())
        {
            // item was just released
            if (!_dollhouse.IsInLineup(gameObject))
            {
                // check if outside dollhouse bounds - project onto 2d x-z plane of floor
                bool isBelowFloor = transform.position.y < _dollhouse._floor.transform.position.y;
                if (isBelowFloor)
                {
                    _dollhouse.AddToLineup(gameObject);
                    return;
                }

                //snap to floor to original / layout position
                var initialPos = _dollhouse.GetInitialPosition(gameObject);
                var newPos = new Vector3(transform.localPosition.x, initialPos.y, transform.localPosition.z);
                transform.localPosition = newPos;
            }
        }
        else
        {
            if (isBeingGrabbed)
            {
                MakeHorizontal();
            }
        }

        // Apply the delta to the corresponding life-size object
        if (lifeSizeObject)
        {
            lifeSizeObject.localPosition += delta * multiplyFactor;
            lifeSizeObject.localRotation = transform.localRotation;
            var p = lifeSizeObject.localPosition;
            lifeSizeObject.localPosition = new Vector3(p.x, (float)initialYLocalPosLifesizeObject, p.z);
        }
    }
}

/*using System;
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

    void EnforceHorizontalRotation()
    {
        float xRotation = -90f; // empirically, what seems to work in our scene
        float yRotation = transform.localRotation.eulerAngles.y;
        float zRotation = transform.localRotation.eulerAngles.z;
        Quaternion rotationQtrn = Quaternion.Euler(xRotation, yRotation, zRotation);
        transform.localRotation = rotationQtrn;
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

        if (!isBeingGrabbed && IsBeingGrabbed() && _dollhouse.IsInLineup(gameObject))
        {
            // object starts to be grabbed
            isBeingGrabbed = IsBeingGrabbed();

            _dollhouse.AddToScene(gameObject);
            return;
        } 
        else if (isBeingGrabbed && !IsBeingGrabbed())
        {
            // trap release of object
            isBeingGrabbed = IsBeingGrabbed();

            if (_dollhouse.IsInLineup(gameObject))
            {
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
                EnforceHorizontalRotation();
                var initialPos = _dollhouse.GetInitialPosition(gameObject);
                var newPos = new Vector3(transform.localPosition.x, initialPos.y, transform.localPosition.z);
                transform.localPosition = newPos;
            }
        }
        else
        {
            isBeingGrabbed = IsBeingGrabbed();
            // enforce horizontal orientation
            EnforceHorizontalRotation();
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
*/