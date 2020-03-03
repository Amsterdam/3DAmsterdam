using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using System.IO;

using cityJSON;
using SimpleJSON;



public class loadCityJSON : MonoBehaviour
{
    
    public Vector3Double Origin = new Vector3Double();
    public string filepath;
    public string filename;
    public Material Defaultmaterial;
    public int LOD = 2;
    private JSONNode json;
    // Start is called before the first frame update
    void Start()
    {
        //CityModel cm = new CityModel();
        //CityObject co = cm.AddBuilding();
        //co.attributes.Add("attribute1", "waarde");
        //SaveCityJSON(cm, "D:/cityjson.json");

        //LoadCityModel("D:/Helsinki/CityGML_BUILDINGS_LOD2_WITHTEXTURES_674496x2/CityGML_BUILDINGS_LOD2_WITHTEXTURES_674496x2.json");
        //LoadCityModel("D:/Helsinki/rotterdam/9300_4340/9300_4340.json");
        //string filename = "D:/Helsinki/DenHaag/8100_4530/8100_4530.json";
        CityModel Citymodel = new CityModel(filepath,filename);
        List<Building> buildings= Citymodel.LoadBuildings(LOD);
        Citymodel = null;
        CreateGameObjects objectcreator = new CreateGameObjects();
        objectcreator.CreateBuildings(buildings, Origin, Defaultmaterial, this.gameObject);

        //LoadCityModel("D:/Helsinki/DenHaag/8100_4530/8100_4530.json");
    }

    


}




