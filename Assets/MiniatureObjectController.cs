using System;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using TMPro;
using UnityEngine;

public class MiniatureObjectController : MonoBehaviour
{
    private Vector3 _initialPosition;
    public Dollhouse dollhouse;
    public Transform lifeSizeObject; // Assign this in the Inspector
    public float multiplyFactor = 8f;
    public double initialYLocalPosLifesizeObject; // this is a hack -- was getting placed too high upon user release
    private bool _isBeingGrabbed = false;
    private HandGrabInteractable _grabInteractable;
    private Vector3 _pinchStartPosition;
    
    private void Start()
    {
        // Store the initial position of this object
        _initialPosition = transform.localPosition;
        _grabInteractable = gameObject.GetComponentInChildren<HandGrabInteractable>();
    }

    // checks if I (mini object) am being grabbed right now
    private bool IsBeingGrabbed()
    {
        if (_grabInteractable != null)
        {
            return _grabInteractable.State == InteractableState.Select;
        }
        return false;
    }

    InteractableState GetGrabState()
    {
        return _grabInteractable.State;
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

    public void OnDrawGizmosOff()
    {
        Vector3 down = -dollhouse.transform.up;
        Ray ray = new Ray(gameObject.transform.position, down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100, ~0);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(ray.origin, ray.direction);
    }

    private void FixedUpdate()
    {
        Vector3 delta = transform.localPosition - _initialPosition;
        _initialPosition = transform.localPosition;

        // trap the release of an object
        var wasBeingGrabbed = _isBeingGrabbed; // was being grabbed in previous update call?
        _isBeingGrabbed = IsBeingGrabbed(); // update variable

        var grabState = GetGrabState();
        if (grabState != InteractableState.Normal)
        {
            Debug.Log($"{grabState} {wasBeingGrabbed}");
        }
        
        if (!wasBeingGrabbed && IsBeingGrabbed())
        {
            // accurately detects pinch on, but i don't know how to cancel the grab
            //    programmatically, so any changes we make to transform are ignored
            Debug.Log("PINCHING ON");
            _pinchStartPosition = transform.localPosition;
        }

        if (wasBeingGrabbed && IsBeingGrabbed())
        {
            if (dollhouse.IsInLineup(gameObject))
            {
                var pinchDisplacement = (transform.localPosition - _pinchStartPosition);
                if (pinchDisplacement.magnitude >= 0.1f)
                {
                    dollhouse.AddToScene(gameObject, false);
                    return;
                }                
            }
        }
        if (wasBeingGrabbed && !IsBeingGrabbed())
        {
            Debug.Log("PINCHING OFF");
            // item was just released
            if (dollhouse.IsInLineup(gameObject))
            {
                // quick pinch-release, add to scene in original position
                MakeHorizontal();
                dollhouse.AddToScene(gameObject);
                return;
            }
            else 
            {
                // shoot a ray down from center position
                Vector3 down = -dollhouse.transform.up;
                Ray ray = new Ray(gameObject.transform.position, down);
                RaycastHit[] hits = Physics.RaycastAll(ray, 100, ~0);
                bool hitsDollhouseFloor = false;
                float rayRange = 1.0f; // Adjust the length of the rays as needed
                
                // note: there are multiple gameobjects in the scene with name matching "FLOOR"
                //   so we do check more precisely
                foreach (RaycastHit hit in hits)
                {
                    GameObject hitObject = hit.transform.gameObject;
                    Debug.Log($"Raycast hit!: {hitObject}");
                    if (hitObject.transform.parent.name.Contains("FLOOR(Clone)"))
                    {
                        Debug.Log("dollhouse.floor HIT");
                        hitsDollhouseFloor = true;
                        break;
                    }
                }

                if (hitsDollhouseFloor)
                {
                    // snap to floor
                    var initialPos = dollhouse.GetInitialPosition(gameObject);
                    var newPos = new Vector3(transform.localPosition.x, initialPos.y, transform.localPosition.z);
                    transform.localPosition = newPos;
                    transform.localRotation = lifeSizeObject.localRotation;
                }
                else
                {
                    dollhouse.AddToLineup(gameObject);
                }
            }
        }
        else
        {
            if (_isBeingGrabbed)
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
    public Dollhouse dollhouse;
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

        if (!isBeingGrabbed && IsBeingGrabbed() && dollhouse.IsInLineup(gameObject))
        {
            // object starts to be grabbed
            isBeingGrabbed = IsBeingGrabbed();

            dollhouse.AddToScene(gameObject);
            return;
        } 
        else if (isBeingGrabbed && !IsBeingGrabbed())
        {
            // trap release of object
            isBeingGrabbed = IsBeingGrabbed();

            if (dollhouse.IsInLineup(gameObject))
            {
            }
            else
            {
                // check if outside dollhouse bounds - project onto 2d x-z plane of floor
                bool isBelowFloor = transform.position.y < dollhouse._floor.transform.position.y;
                if (isBelowFloor)
                {
                    dollhouse.AddToLineup(gameObject);
                    return;
                }

                //snap to floor
                EnforceHorizontalRotation();
                var initialPos = dollhouse.GetInitialPosition(gameObject);
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