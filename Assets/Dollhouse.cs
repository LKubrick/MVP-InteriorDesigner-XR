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
    [SerializeField] private GameObject _leftHandAnchor;
    [SerializeField] private Vector3 offsetFromLeftHand = new Vector3(1, 1, 1);
    [SerializeField] private float _scalingFactor;
    [SerializeField] private List<String> _namesToBuildFully;
    [SerializeField] private List<String> _namesToBuildDollhouseOnly;
    
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
   
    private void FixedUpdate()
    {
        Vector3 newPosition = _leftHandAnchor.transform.position + offsetFromLeftHand;
        _dollhouseOrigin.transform.position = newPosition;
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
                    Debug.Log("skipping...do not build {obj.name}...deleting big object version");
                    Destroy(obj);
                    continue;
                }

                if (isBuildDollhouseOnly)
                {
                    Debug.Log("building dollhouse only...deleting big object version");
                    Destroy(obj);
                }
                Debug.Log($"trying to clone {obj.name}");
                GameObject prefab = null;
                GameObject newMiniObj = null;
                newMiniObj = Instantiate(obj);
                
                newMiniObj.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
                newMiniObj.transform.localPosition *= _scalingFactor;
                
                newMiniObj.transform.SetParent(_dollhouseOrigin.transform);
                
                // prepare the new object
                // add grabbable, rigidbody (to top-level object), and paired movement
                var topMiniObj = newMiniObj;
                Rigidbody miniRb = null;
                bool isMakeGrabbable = isBuildFully;
                if (isMakeGrabbable)
                {
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
                }
            }
        }

        Debug.Log($"{cnt} objects cloned...scaling down...");
        _dollhouseOrigin.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
    }
}
