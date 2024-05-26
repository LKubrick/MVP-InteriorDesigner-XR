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
        //public Vector3 spawnDistance = new Vector3(0,2,0);
        public Material dollhouseMaterial;
        
    void Start()
    {
        StartCoroutine(SpawnDollhouse());
    }

    private IEnumerator SpawnDollhouse()
    {
        yield return new WaitForSeconds(2f);
        
        GameObject bed = GameObject.FindWithTag(bedTag);
        GameObject desk = GameObject.FindWithTag(tableTag);
        GameObject[] walls = GameObject.FindGameObjectsWithTag(wallTag);
        
        GameObject dollhouseBed = Instantiate(bed, dollhouseOrigin.transform.position, bed.transform.rotation);
        dollhouseBed.transform.localScale = bed.transform.localScale * scale;
        
        GameObject dollhouseDesk = Instantiate(desk, dollhouseOrigin.transform.position, desk.transform.rotation);
        dollhouseDesk.transform.localScale = desk.transform.localScale * scale;
        
        dollhouseBed.transform.position = dollhouseOrigin.transform.position + (bed.transform.position - dollhouseOrigin.transform.position) * scale;
        dollhouseDesk.transform.position = dollhouseOrigin.transform.position + (desk.transform.position - dollhouseOrigin.transform.position) * scale;

        foreach (GameObject wall in walls)
        {
            // Duplicate the wall GameObject
            GameObject dollhouseWall = Instantiate(wall, dollhouseOrigin.transform.position, wall.transform.rotation);

            // Set the scale of the duplicated wall
            dollhouseWall.transform.localScale = wall.transform.localScale * scale;

            // Adjust the position of the duplicated wall relative to the dollhouse origin
            dollhouseWall.transform.position = dollhouseOrigin.transform.position +
                                               (wall.transform.position - dollhouseOrigin.transform.position) * scale;

            MeshRenderer wallRenderer = dollhouseWall.GetComponentInChildren<MeshRenderer>();

            if (wallRenderer != null)
            {
                wallRenderer.material = dollhouseMaterial;
            }


        }
    }
}
