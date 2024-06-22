using System;
using System.Linq;
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
    [SerializeField] private OVRPassthroughLayer _passthrough;
    [SerializeField] private GameObject _lineupHolder;
    [SerializeField] private GameObject _buttonA;
    [SerializeField] private GameObject _buttonB;
    [SerializeField] private GameObject _buttonC;
    [SerializeField] private GameObject _buttonXR;
    [SerializeField] private TMP_Text _debugText;
    
    private List<GameObject> dollhouseOnlyLargeItems = new List<GameObject>();
    private Vector3 lineupRotVector;
    private bool isLineupRotVectorDefined = false;
    private Dictionary<GameObject,Vector3> _initialPositionsForMiniObj;
    private Dictionary<GameObject,Quaternion> _initialRotationsForMiniObj;
    
    // each mini obj is always either in lineup or scene
    private List<GameObject> _lineup = new List<GameObject>();  //  items left out of dollhouse
    private List<GameObject> _scene = new List<GameObject>(); // items placed in the dollhouse
    
    private bool isFirstTime = true;
    public GameObject _floor;
    float _buttonPressStartTime;

    // Start is called before the first frame update
    void Start()
    {
        _initialPositionsForMiniObj = new Dictionary<GameObject,Vector3>();
        _initialRotationsForMiniObj = new Dictionary<GameObject,Quaternion>();
    }

    IEnumerable<GameObject> GetAllMiniObjects()
    {
        return _lineup.Concat(_scene); // clones the list
    }

    public void OnButtonSelect(GameObject button)
    {
        _debugText.text = $"Select {button} Xr: {_buttonXR} a: {_buttonA}";
        if (button == _buttonXR)
        {
            _debugText.text += "toggle vr";
            ToggleVRMode();
        }
        _buttonPressStartTime = Time.time;
    }

    public void OnButtonRelease(GameObject button)
    {
        _debugText.text = $"Release {button} Xr: {_buttonXR} a: {_buttonA}";
        int layoutIdx = -1;
        if (button == _buttonXR)
        {
            return;
        } 
        else if (button == _buttonA)
        {
            layoutIdx = 0;
        }
        else if (button == _buttonB)
        {
            layoutIdx = 1;
        } 
        else if (button == _buttonC)
        {
            layoutIdx = 2;
        }

        if (layoutIdx > -1)
        {
            var timeElapsed = Time.time - _buttonPressStartTime;
            _debugText.text += $"idx: {layoutIdx} elapsed: {timeElapsed}";
            if (timeElapsed > 1f)
            {
                SaveLayout(layoutIdx);
            }
            else
            {
                LoadLayout(layoutIdx);
            }
        }
    }

    
    // layout state
    struct LayoutData
    {
        public Vector3 pos { get; set; }
        public Quaternion rot { get; set; }
    }
    Dictionary<GameObject, LayoutData>[] savedLayouts = new Dictionary<GameObject, LayoutData>[3];

    void SaveLayout(int layoutIdx)
    {
        savedLayouts[layoutIdx] = new Dictionary<GameObject, LayoutData>();
        foreach (var obj in _scene)
        {
            LayoutData layoutData = new LayoutData
            {
                pos = obj.transform.localPosition, rot = obj.transform.localRotation
            };
            savedLayouts[layoutIdx][obj] = layoutData;
        }

        var cnt = savedLayouts[layoutIdx].Count();
        Debug.Log($"savedLayouts: {cnt}");
    }
    
    void LoadLayout(int layoutIdx)
    {
        // clone the list bc we will clear lineup and scene immediately
        var allObj = new List<GameObject>(GetAllMiniObjects());
        _lineup.Clear();
        _scene.Clear();
        foreach (var x in allObj)
        {
            AddToLineup(x);

            if (savedLayouts[layoutIdx].ContainsKey(x))
            {
                Debug.Log($"saved layout found for {x} adding to scene");
                var data = savedLayouts[layoutIdx][x];
                _initialPositionsForMiniObj[x] = data.pos;
                _initialRotationsForMiniObj[x] = data.rot;
                AddToScene(x);
            }
        }
        ArrangeLineup();
    }
    // trigger events from unity editor
    void DebugHotKeys()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_lineup.Count() > 0)
            {
                AddToScene(_lineup.First());
                ArrangeLineup();
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            AddToLineup(_scene.First(), true);
        }
        
        if (Input.GetKeyDown(KeyCode.A)) // replay layout
        {
            LoadLayout(0);
        }
        
        if (Input.GetKeyDown(KeyCode.Z)) // save layout
        {
            SaveLayout(0);
        }
        
        if (Input.GetKeyDown(KeyCode.S)) // replay layout
        {
            LoadLayout(1);
        }
        
        if (Input.GetKeyDown(KeyCode.X)) // save layout
        {
            SaveLayout(1);
        }
        if (Input.GetKeyDown(KeyCode.D)) // replay layout
        {
            LoadLayout(2);
        }
        
        if (Input.GetKeyDown(KeyCode.C)) // save layout
        {
            SaveLayout(2);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        DebugHotKeys();
        if (isFirstTime)
        {
            StartCoroutine(BuildDollhouse());
            isFirstTime = false;
        }
    }

    public void AddToLineup(GameObject x, bool callArrangeLineup = true)
    {
        _lineup.Add(x);
        _scene.Remove(x);
        var rb = x.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        var miniObjCtrl = x.GetComponentInChildren<MiniatureObjectController>();
        var lifesizeObj = miniObjCtrl.lifeSizeObject.gameObject;
        lifesizeObj.SetActive(false);

        if (callArrangeLineup)
        {
            ArrangeLineup();
        }
    }

    public Vector3 GetInitialPosition(GameObject x)
    {
        return _initialPositionsForMiniObj[x];
    }

    private bool isVRMode = true;
    public void ToggleVRMode()
    {
        isVRMode = !isVRMode;
        
        var passthroughpct = 1f;
        bool isActiveFlag = false;
        if (isVRMode)
        {
            passthroughpct = 0f;
            isActiveFlag = true;
        }
        _passthrough.textureOpacity = passthroughpct;
        foreach (GameObject x in dollhouseOnlyLargeItems)
        {
            x.SetActive(isActiveFlag);
        }
    }
    
    void SetMiniObjInitialTransforms()
    {
        foreach (GameObject x in _lineup)
        {
            Transform trans = x.transform;
            Debug.Log($"setting initial transform for {x.name} -> {x.transform.position}");
            _initialPositionsForMiniObj[x] = trans.localPosition;
            _initialRotationsForMiniObj[x] = trans.localRotation;
        }
    }
    private void ArrangeLineup()
    {
        Debug.Log($"Lineup: {_lineup.Count()}  Scene: {_scene.Count()}");
        float spacer = .05f; // at room scale
        Vector3 lineupOriginPos = _dollhouseOrigin.transform.position;
        float rotationAngle = -90f;

        if (!isLineupRotVectorDefined)
        {
            lineupRotVector = Quaternion.AngleAxis(rotationAngle, Vector3.up)
                              * Camera.main.transform.forward;
            isLineupRotVectorDefined = true;
        }

        lineupOriginPos = new Vector3(lineupOriginPos.x, 
            lineupOriginPos.y, lineupOriginPos.z);
        lineupOriginPos += lineupRotVector * 0.5f;
        
        // XXX sort lineup by order in _namesToBuildFully
        
        // calculate the lineup width empirically
        var myCursor = lineupOriginPos;
        foreach (GameObject x in _lineup)
        {
            var newPos = myCursor;
            //x.transform.position = newPos;
            var renderer = x.GetComponentInChildren<MeshRenderer>();
            var objWidth = renderer.bounds.size.x;
            myCursor -= lineupRotVector * (objWidth + spacer);
        }
        var lineupWidthVector = myCursor - lineupOriginPos;
        var lineupWidth = lineupWidthVector.magnitude;

        // center the lineup
        lineupOriginPos = _dollhouseOrigin.transform.position;
        lineupOriginPos += lineupRotVector * 0.5f * lineupWidth;

        // layout the mini furniture
        myCursor = lineupOriginPos;
        foreach (GameObject x in _lineup)
        {
            var newPos = myCursor;
            x.transform.position = newPos;
            var renderer = x.GetComponentInChildren<MeshRenderer>();
            var objWidth = renderer.bounds.size.x;
            MakeHorizontal(x.transform);
            myCursor -= lineupRotVector * (objWidth + spacer);
        }

        //adjust the lineup holder
        var temp = _lineupHolder.transform.localScale;
        _lineupHolder.transform.localScale = new Vector3(4f * lineupWidth / _dollhouseOrigin.transform.localScale.x, 0.1f, temp.z);
    }
    void MakeHorizontal(Transform _transform)
    {
        float xRotation = -90f; // empirically, what seems to work in our scene
        float yRotation = _transform.localRotation.eulerAngles.y;
        float zRotation = _transform.localRotation.eulerAngles.z;
        Quaternion rotationQtrn = Quaternion.Euler(xRotation, yRotation, zRotation);
        _transform.localRotation = rotationQtrn;
    }
    public bool IsInLineup(GameObject obj)
    {
        return _lineup.Contains(obj);
    }

    public void AddToScene(GameObject miniObj)
    {
        Debug.Log($"AddToScene: {miniObj.name}");
        _lineup.Remove(miniObj);
        var origPos = _initialPositionsForMiniObj[miniObj];
        var origRot = _initialRotationsForMiniObj[miniObj];
        Debug.Log($"AddToScene: setting initial transform for {miniObj.name} -> {origPos} {origRot}");

        miniObj.transform.localPosition = origPos;
        miniObj.transform.localRotation = origRot;
        
        var rb = miniObj.GetComponent<Rigidbody>();
        //rb.isKinematic = false; keep it kinematic the whole time

        var miniObjCtrl = miniObj.GetComponentInChildren<MiniatureObjectController>();
        var lifesizeObj = miniObjCtrl.lifeSizeObject.gameObject;
        lifesizeObj.SetActive(true);
        _scene.Add(miniObj);
    }

    // do this to make sure coords are set correctly after BuildDollhouse() is called
    IEnumerator ArrangeInitialLineup()
    {
        yield return new WaitForSeconds(0.5f);
        SetMiniObjInitialTransforms(); 
        ArrangeLineup();
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
                    Debug.Log($"skipping...do not build {obj.name}...destroying big object version");
                    Destroy(obj);
                    continue;
                } 
                else if (isBuildDollhouseOnly)
                {
                    Debug.Log($"building dollhouse only for {obj.name}...deleting big object version");
                    dollhouseOnlyLargeItems.Add(obj);
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
                    ctrl._dollhouse = this;
                    ctrl.initialYLocalPosLifesizeObject = obj.transform.localPosition.y;
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
                    
                    AddToLineup(newMiniObj, false);
                }
                
                // treat FLOOR specially
                if (nameToSearch.Contains("FLOOR"))
                {
                    _floor = topMiniObj;
                }
            }
        }

        Debug.Log($"{cnt} objects cloned...scaling down...");
        _dollhouseOrigin.transform.localScale = new Vector3(_scalingFactor, _scalingFactor, _scalingFactor);

        StartCoroutine(ArrangeInitialLineup());
    }
}
