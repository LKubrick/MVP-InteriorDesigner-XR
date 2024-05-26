using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollHouseBuilder : MonoBehaviour
{
        public string bedTag = "Bed";
        public string tableTag = "Table";
        public string wallTag = "Wall";
        
        public float scale = 0.1f;

        public GameObject dollhouseOrigin; 
        public Material dollhouseMaterial;
        public GameObject lefthandAnchor;

        private GameObject _bed;
        private GameObject _miniBed;

        private GameObject _table;
        private GameObject _miniTable;
        
    void Start()
    {
        StartCoroutine(SpawnDollhouse());
    }

    private IEnumerator SpawnDollhouse()
    {
        yield return new WaitForSeconds(2f);
        
        //float yPosition = lefthandAnchor.transform.position.y;
        //Vector3 newOriginPosition = new Vector3(0f, yPosition +0.15f, 0.5f);
        //dollhouseOrigin.transform.position = newOriginPosition;
        
        _bed = GameObject.FindWithTag(bedTag);
        _table = GameObject.FindWithTag(tableTag);
        GameObject[] walls = GameObject.FindGameObjectsWithTag(wallTag);
        
        _miniBed = Instantiate(_bed, dollhouseOrigin.transform.position, _bed.transform.rotation);
        _miniBed.transform.localScale = _bed.transform.localScale * scale;
        _miniBed.transform.parent = dollhouseOrigin.transform;
        
        _miniTable = Instantiate(_table, dollhouseOrigin.transform.position, _table.transform.rotation);
        _miniTable.transform.localScale = _table.transform.localScale * scale;
        _miniTable.transform.parent = dollhouseOrigin.transform;
        
        _miniBed.transform.position = dollhouseOrigin.transform.position + (_bed.transform.position - dollhouseOrigin.transform.position) * scale;
        _miniTable.transform.position = dollhouseOrigin.transform.position + (_table.transform.position - dollhouseOrigin.transform.position) * scale;

        foreach (GameObject wall in walls)
        {
            // Duplicate the wall GameObject
            GameObject dollhouseWall = Instantiate(wall, dollhouseOrigin.transform.position, wall.transform.rotation);

            // Set the scale of the duplicated wall
            dollhouseWall.transform.localScale = wall.transform.localScale * scale;

            // Adjust the position of the duplicated wall relative to the dollhouse origin
            dollhouseWall.transform.position = dollhouseOrigin.transform.position +
                                               (wall.transform.position - dollhouseOrigin.transform.position) * scale;
            
            dollhouseWall.transform.parent = dollhouseOrigin.transform;

            MeshRenderer wallRenderer = dollhouseWall.GetComponentInChildren<MeshRenderer>();

            if (wallRenderer != null)
            {
                wallRenderer.material = dollhouseMaterial;
            }


        }
    }

    private void FixedUpdate()
    {
        float xPosition = lefthandAnchor.transform.position.x;
        float zPosition = lefthandAnchor.transform.position.z;
        Vector3 newOriginPosition = new Vector3(xPosition, 1f, zPosition);
        dollhouseOrigin.transform.position = newOriginPosition;
        
        _bed.transform.position = dollhouseOrigin.transform.position + (_miniBed.transform.position - dollhouseOrigin.transform.position) / scale;
        _bed.transform.rotation = _miniBed.transform.rotation;
        
        _table.transform.position = dollhouseOrigin.transform.position + (_miniTable.transform.position - dollhouseOrigin.transform.position) / scale;
        _table.transform.rotation = _miniTable.transform.rotation;
    }
}
