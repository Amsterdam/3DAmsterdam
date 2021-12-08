
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;

public class CityJsonVisualiser : MonoBehaviour
{
    public Material MeshMaterial;

    public double minx;
    public double miny;
    public double minz;

    public double maxx;
    public double maxy;
    public double maxz;


    public double centerx;
    public double centery;
    public double centerz;

    void Start()
    {
        Netherlands3D.T3D.Uitbouw.MetadataLoader.Instance.BimCityJsonReceived += OnBimCityJsonReceived;
    }

    private void OnBimCityJsonReceived(string cityJson)
    {
        Debug.Log(cityJson);

        var cityJsonModel = new CityJsonModel(cityJson);

        
        var meshmaker = new CreateMeshFromCityJson();
        
        var mesh = meshmaker.CreateMesh(cityJsonModel);

        //var verts = mesh.vertices;
        var verts = cityJsonModel.vertices;

        minx = verts.Min(o => o.x);
        miny = verts.Min(o => o.y);
        minz = verts.Min(o => o.z);

        maxx = verts.Max(o => o.x);
        maxy = verts.Max(o => o.y);
        maxz = verts.Max(o => o.z);

        centerx = minx + ((maxx - minx) / 2);
        centery = miny + ((maxy - miny) / 2);
        centerz = minz + ((maxz - minz) / 2);

        //for (int i = 0; i < verts.Length; i++)
        //{
        //    verts[i].x -= centerx;
        //    verts[i].z -= centerz;
        //}

        //mesh.vertices = verts;


        var meshfilter = gameObject.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;

        var meshrenderer = gameObject.AddComponent<MeshRenderer>();
        meshrenderer.material = MeshMaterial;


    }

    
}
