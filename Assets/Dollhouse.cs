using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using Oculus.Interaction.Grab;
using Oculus.Interaction.HandGrab;
using TMPro;
using UnityEngine;

public class Dollhouse : MonoBehaviour
{
    [SerializeField] private GameObject _dollhouseOrigin;
    [SerializeField] private float _scalingFactor;
    [SerializeField] private List<String> _namesToBuildFully;
    [SerializeField] private List<String> _namesToBuildDollhouseOnly;

    private List<GameObject> _lineup = new List<GameObject>();  // furniture items left out of dollhouse
    private bool isFirstTime = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isFirstTime)
        {
            StartCoroutine(BuildDollhouse());
            isFirstTime = false;
        }
    }

    void AddToLineup(GameObject x)
    {
        _lineup.Add(x);
        var rb = x.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        
        //ArrangeLineup();
    }

    private Vector3 gizmoPt1 = new Vector3(0f, 0f, 0f);
    public Vector3 gizmoPt2 = new Vector3(0f,0f,0f);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(gizmoPt1, gizmoPt2);
    }

    void ArrangeLineup()
    {
        Debug.Log($"Lineup: ");
        foreach (GameObject x in _lineup)
        {
            Debug.Log($"{x.name}");
        }
        float spacer = .05f; // at room scale
        Transform lineupOrigin = _dollhouseOrigin.transform;
        float rotationAngle = -90f;
        Vector3 lineupRotVector = Quaternion.AngleAxis(rotationAngle, Vector3.up)
            * Camera.main.transform.forward;
        lineupOrigin.transform.position = new Vector3(lineupOrigin.position.x, 
            lineupOrigin.position.y, lineupOrigin.position.z);
        lineupOrigin.transform.position += lineupRotVector * 0.5f;
        
        // XXX need to calculate total length of lineup, then recenter
        
        // XXX sort lineup by order in _namesToBuildFully

        // enforce position in lineup
        var myCursor = lineupOrigin.position;
        foreach (GameObject x in _lineup)
        {
            var newPos = myCursor;
            x.transform.position = newPos;
            var renderer = x.GetComponentInChildren<MeshRenderer>();
            var objWidth = renderer.bounds.size.x;
            myCursor -= lineupRotVector * (objWidth + spacer);
        }
    }
    
    IEnumerator BuildDollhouse()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("trying to build dollhouse");
        
        int cnt = 0;
        foreach (var room in MRUK.Instance.GetRooms())
        {
            foreach (var anchor in room.GetRoomAnchors())
            {
                isFirstTime = false;
                var obj = anchor.gameObject;
                var nameToSearch = obj.name.ToUpper();

                bool isBuildFully = false;
                bool isBuildDollhouseOnly = false;

                // search lists for inclusion
                foreach (String name in _namesToBuildFully)
                {
                    if (nameToSearch.ToUpper().Contains(name.ToUpper()))
                    {
                        isBuildFully = true;
                    }
                }
                foreach (String name in _namesToBuildDollhouseOnly)
                {
                    if (nameToSearch.ToUpper().Contains(name.ToUpper()))
                    {
                        isBuildDollhouseOnly = true;
                    }
                }
                if (!(isBuildFully || isBuildDollhouseOnly))
                {
                    Debug.Log($"skipping...do not build {obj.name}...deleting big object version");
                    Destroy(obj);
                    continue;
                } 
                else if (isBuildDollhouseOnly)
                {
                    Debug.Log($"building dollhouse only for {obj.name}...deleting big object version");
                    Destroy(obj);
                }

                Debug.Log($"trying to clone {obj.name}");
                GameObject prefab = null;
                GameObject newMiniObj = null;
                newMiniObj = Instantiate(obj);
                
                newMiniObj.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
                newMiniObj.transform.localPosition *= _scalingFactor;
                newMiniObj.transform.SetParent(_dollhouseOrigin.transform);

                Debug.Log($"Initial position {nameToSearch}: {obj.transform.position}");

                // prepare the new object
                // add grabbable, rigidbody (to top-level object), and paired movement
                var topMiniObj = newMiniObj;
                Rigidbody miniRb = null;
                bool isMakeGrabbable = isBuildFully;
                if (isMakeGrabbable)
                {
                    Debug.Log($"make grabbable {obj.name}");

                    var grab = topMiniObj.AddComponent<Grabbable>();
                    grab.InjectOptionalKinematicWhileSelected(true);
                    grab.InjectOptionalThrowWhenUnselected(true);
                    
                    miniRb = topMiniObj.AddComponent<Rigidbody>();
                    miniRb.useGravity = true;
                    miniRb.mass = 1;
                    
                    miniRb.isKinematic = false;
                    miniRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    
                    // allow paired-movement, mini -> large obj
                    var ctrl = newMiniObj.AddComponent<MiniatureObjectController>();
                    ctrl.multiplyFactor = 1.0f / _scalingFactor;
                    ctrl.lifeSizeObject = obj.transform;
                }

                var miniMeshRenderer = topMiniObj.gameObject.GetComponentInChildren<MeshRenderer>();
                if (!miniMeshRenderer)
                {
                    throw new Exception("no MeshRenderer found under this parent");
                }
                var miniPrefab = miniMeshRenderer.gameObject;
                if (miniPrefab == null)
                {
                    throw new Exception("miniprefab GameObject of MeshRenderer is null");
                }
                // add collider (to top level object)
                var collider = topMiniObj.AddComponent<BoxCollider>();
                if (!collider)
                {
                    throw new Exception("could not instantiate Collider for miniPrefab");
                }
                if (isMakeGrabbable)
                {
                    IPointableElement miniPrefabPointableElement = topMiniObj.GetComponent<IPointableElement>();
                    if (miniPrefabPointableElement == null)
                    {
                        throw new Exception("no IPointableElement found");
                    }
                    // add HandgrabInteractable (to child of parent)
                    var midMiniObj = new GameObject();
                    var midMiniObjHgi = midMiniObj.AddComponent<HandGrabInteractable>();
                    
                    midMiniObjHgi.InjectRigidbody(miniRb);
                    midMiniObjHgi.InjectSupportedGrabTypes(GrabTypeFlags.All);
                    midMiniObjHgi.InjectOptionalPointableElement(miniPrefabPointableElement);
                    midMiniObjHgi.transform.SetParent(topMiniObj.transform);
                    
                    AddToLineup(newMiniObj);
                }
            }
        }

        Debug.Log($"{cnt} objects cloned...scaling down...");
        _dollhouseOrigin.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
        
        ArrangeLineup();
    }
}
