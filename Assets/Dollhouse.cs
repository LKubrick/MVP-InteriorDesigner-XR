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
    [SerializeField] private GameObject _prefabParent;
    [SerializeField] private HandGrabInteractable _handInteractableExemplar;
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

    IEnumerator BuildDollhouse()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("trying to build dollhouse");
        
        int cnt = 0;
        foreach (var room in MRUK.Instance.GetRooms())
        {
            foreach (var anchor in room.GetRoomAnchors())
            {
                isFirstTime = false;
                var obj = anchor.gameObject;
                if (obj.name.Contains("CEILING") || obj.name.Contains("GLOBAL_MESH"))
                {
                    Debug.Log($"Skipping {obj.name}");
                    continue;
                }

                Debug.Log($"trying to clone {obj.name}");
                var nameToSearch = obj.name;
                GameObject prefab = null;
                
                
                // find the equivalent prefab
                foreach (Transform child in _prefabParent.transform)
                {
                    // based on v65 Sample scene MRUK: Virtual Home
                    // prefab hierarchy:
                    //   the useable "sofa" prefab w/ MeshRenderer is below the "sofa" placeholder parent
                    
                    if (nameToSearch.ToLower().Contains(child.gameObject.name.ToLower()))
                    {
                        Debug.Log($"prefab {child.gameObject.name} found for {obj.name}");
                        var meshRenderer = child.gameObject.GetComponentInChildren<MeshRenderer>();
                        if (!meshRenderer)
                        {
                            throw new Exception("no MeshRenderer found under this parent");
                        }
                        prefab = meshRenderer.gameObject;
                        if (prefab == null)
                        {
                            throw new Exception("prefab GameObject of MeshRenderer is null");
                        }

                        break;
                    }
                }

                GameObject newMiniObj = null;
                if (prefab != null)
                {
                    newMiniObj = Instantiate(prefab, obj.transform.position, obj.transform.rotation);
                    newMiniObj.transform.localScale = obj.GetComponent<Renderer>().bounds.size;
                    newMiniObj.transform.SetParent(_dollhouseOrigin.transform);

                    var miniObjCtrl = newMiniObj.GetComponent<MiniatureObjectController>();
                    miniObjCtrl.multiplyFactor = 1.0f / _scalingFactor;
                    miniObjCtrl.lifeSizeObject = prefab.transform;
                }
                else
                {
                    newMiniObj = Instantiate(obj);
                    
                    newMiniObj.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
                    newMiniObj.transform.localPosition *= _scalingFactor;
                    
                    newMiniObj.transform.SetParent(_dollhouseOrigin.transform);
//                    var ctrl = newMiniObj.AddComponent<MiniatureObjectController>();
//                    ctrl.multiplyFactor = 1.0f / _scalingFactor;
//                    ctrl.lifeSizeObject = obj.transform;
                }
                
                // prepare the new object
                // add grabbable, rigidbody (to top-level object)
                var topMiniObj = newMiniObj;
                Rigidbody miniRb = null;
                if (!(obj.name.Contains("WALL") || obj.name.Contains("FLOOR")))
                {
                    var grab = topMiniObj.AddComponent<Grabbable>();
                    grab.InjectOptionalKinematicWhileSelected(true);
                    grab.InjectOptionalThrowWhenUnselected(true);
                    
                    miniRb = topMiniObj.AddComponent<Rigidbody>();
                    miniRb.useGravity = true;
                    miniRb.mass = 1;
                    
                    miniRb.isKinematic = false;
                    miniRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
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

                // rigidbody? Then we want to make it grabbable
                if (miniRb)
                {
                    IPointableElement miniPrefabPointableElement = topMiniObj.GetComponent<IPointableElement>();
                    if (miniPrefabPointableElement == null)
                    {
                        throw new Exception("no IPointableElement found");
                    }
                    // add HandgrabInteractable (to child of parent)
                    var midMiniObj = Instantiate(_handInteractableExemplar);
                    
                    midMiniObj.InjectRigidbody(miniRb);
                    midMiniObj.InjectSupportedGrabTypes(GrabTypeFlags.All);
                    midMiniObj.InjectOptionalPointableElement(miniPrefabPointableElement);
                    midMiniObj.transform.SetParent(topMiniObj.transform);
                }
            }
        }

        Debug.Log($"{cnt} objects cloned...scaling down...");
        _dollhouseOrigin.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
    }
}
