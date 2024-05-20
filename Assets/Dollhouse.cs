using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class Dollhouse : MonoBehaviour
{
    [SerializeField] private GameObject _dollhouseOrigin;
    [SerializeField] private float _scalingFactor;
    [SerializeField] private GameObject _prefabParent;
    [SerializeField] private GameObject _prefabFiller;  // cube object to stand in for a realistic prefab

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
                if (obj.name.Contains("CEILING"))
                {
                    Debug.Log($"Skipping {obj.name}");
                    continue;
                }

                Debug.Log($"trying to clone {obj.name}");
                var nameToSearch = obj.name;
                GameObject prefab = null;
                
                /*
                // find the equivalent prefab
                foreach (Transform child in _prefabParent.transform)
                {
                    // prefab hierarchy: the useable "sofa" prefab w/ Renderer is below the "sofa" placeholder parent
                    if (nameToSearch.ToLower().Contains(child.gameObject.name.ToLower()))
                    {
                        Debug.Log($"prefab {child.gameObject.name} found for {obj.name}");
                        var fancyResizable = child.gameObject.GetComponentInChildren<FancyResizable>();
                        if (!fancyResizable)
                        {
                            throw new Exception("no FancyResizable found for this object");
                        }
                        prefab = fancyResizable.gameObject;
                        if (prefab == null)
                        {
                            throw new Exception("prefab is null");
                        }
                        if (!prefab.GetComponent<Renderer>())
                        {
                            throw new Exception("no Renderer found for this object");
                        }

                        break;
                    }
                }*/
                prefab = null; // XXX hack for now
                GameObject newLargeObj = null;
                GameObject newMiniObj = null;
                if (prefab != null)
                {
                    //var renderer = prefab.GetComponent<Renderer>();
                    //newLargeObj = _resizer.CreateResizedObject(renderer.bounds.size, obj, prefab);
                    //newMiniObj = _resizer.CreateResizedObject(renderer.bounds.size, _dollhouseOrigin, prefab);
                    newLargeObj = Instantiate(prefab, obj.transform.position, obj.transform.rotation);
                    var txt = newLargeObj.GetComponentInChildren<TMP_Text>();
                    txt.text = obj.name;
                    newLargeObj.transform.localScale = obj.GetComponent<Renderer>().bounds.size;

                    newMiniObj = Instantiate(prefab, obj.transform.position, obj.transform.rotation);
                    var txt2 = newMiniObj.GetComponentInChildren<TMP_Text>();
                    txt2.text = obj.name;
                    newMiniObj.transform.localScale = obj.GetComponent<Renderer>().bounds.size;
                    newMiniObj.transform.SetParent(_dollhouseOrigin.transform);

                    var miniObjCtrl = newMiniObj.GetComponent<MiniatureObjectController>();
                    miniObjCtrl.multiplyFactor = 1.0f / _scalingFactor;
                    miniObjCtrl.lifeSizeObject = newLargeObj.transform;
                }
                else
                {
                    //newLargeObj = Instantiate(obj);
                    newMiniObj = Instantiate(obj);
                    
                    newMiniObj.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
                    newMiniObj.transform.localPosition *= _scalingFactor;
                    
                    newMiniObj.transform.SetParent(_dollhouseOrigin.transform);
                    
                    var rb = newMiniObj.AddComponent<Rigidbody>();
                    rb.isKinematic = true;

                    var lowerCaseName = obj.name.ToLower();
                    if (!(lowerCaseName.Contains("floor") || lowerCaseName.Contains("wall")))
                    {
                        var gb = newMiniObj.AddComponent<Grabbable>();
                    }
                    
//                    var ctrl = newMiniObj.AddComponent<MiniatureObjectController>();
//                    ctrl.multiplyFactor = 1.0f / _scalingFactor;
//                    ctrl.lifeSizeObject = obj.transform;

                }
            }
        }

        Debug.Log($"{cnt} objects cloned...scaling down...");
        _dollhouseOrigin.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);
    }
}
