using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cityJSON;
public class CityJSONSettings : MonoBehaviour
{
    public Vector3Double Origin = new Vector3Double();
    public string filepath;
    public string filename;
    public Material Defaultmaterial;
    public int LOD = 2;
    public string modelname;

    public bool useTextures = true;
    public bool SingleMeshBuildings = false;
    public bool minimizeMeshes = true;
    public bool CreatePrefabs = false;
}
