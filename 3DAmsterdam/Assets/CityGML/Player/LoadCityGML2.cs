using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CityGML;
using System.Xml;

public class LoadCityGML2 : MonoBehaviour
{
    public Material DefaultMaterial;
    
    [SerializeField]
    public Vector3Double Origin = new Vector3Double();
    //origin Helsinki = 25500000,6665237,0
    public string filepath = "D:/CityGmlfromDBV2/tile_9_19/";
    public string filename = "gebouwenLOD2.gml";
    public int LOD = 2;
    public bool LoadTextures;

    public CreateGameObjects ObjectBuilder;
    private XmlDocument doc;
    void Start()
    {
        //string filename = "D:/CityGmlfromDBV2/tile_9_19/gebouwenLOD2.gml";
        //filename = "D:/Helsinki/CityGML_BUILDINGS_LOD2_WITHTEXTURES_664502x2/CityGML_BUILDINGS_LOD2_WITHTEXTURES_664502x2/CityGML_BUILDINGS_LOD2_WITHTEXTURES_664502x2.gml";
        CityGML.CityModel citymodel= new CityGML.CityModel(filepath, filename);
        StartCoroutine(CreateBuildingGameObjects(citymodel.GetBuildings(LOD)));
        Debug.Log("done");
    }

    private IEnumerator CreateBuildingGameObjects(List<BuildingFeature> Buildings)
    {
        foreach (BuildingFeature building in Buildings)
        {
            GameObject BuildingObject;
            BuildingObject = ObjectBuilder.CreateBuilding(building, Origin, this.gameObject, DefaultMaterial, LoadTextures);
            yield return null;
        }
    }

   
}
