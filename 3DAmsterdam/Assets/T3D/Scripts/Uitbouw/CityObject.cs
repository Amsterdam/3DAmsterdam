using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.Assertions;
using SimpleJSON;

public enum CityObjectType
{
    // 1000-2000: 1st level city objects
    Building = 1000,
    Bridge = 1010,
    CityObjectGroup = 1020,


    Tunnel = 1130,

    //2000-3000: 2nd level city objects. the middle numbers indicates the required parent. e.g 200x has to be a parent of 1000, 201x of 1010 etc.
    BuildingPart = 2000,
    BuildingInstallation = 2001,
    BridgePart = 2010,
    BridgeInstallation = 2011,
    BridgeConstructionElement = 2012,

    TunnelPart = 2130,
    TunnelInstallation = 2131
}

public class CityObject : MonoBehaviour
{
    public string Name;
    public CityObjectType Type;
    public CityObject[] Parents;

    //public CityObject Parents
    //{
    //    get
    //    {
    //        return parents;
    //    }
    //    private set
    //    {
    //        Assert.IsTrue(IsValidParent(this, value));
    //        parents = value;
    //    }
    //}

    public int Lod { get; protected set; } = 1;
    public CityPolygon[] Polygons;

    public static bool IsValidParent(CityObject child, CityObject parent)
    {
        if (parent == null && ((int)child.Type < 2000))
            return true;

        if ((int)((int)child.Type / 10 - 200) == (int)((int)parent.Type / 10 - 100))
            return true;


        Debug.Log(child.Type + "\t" + parent, child.gameObject);
        return false;
    }

    public JSONObject GetJsonObject()
    {
        var obj = new JSONObject();
        obj["type"] = Type.ToString();
        if (Parents.Length > 0)
        {
            var parents = new JSONArray();
            for (int i = 0; i < Parents.Length; i++)
            {
                Assert.IsTrue(IsValidParent(this, Parents[i]));
                parents[i] = Parents[i].Name;
            }
            obj["parents"] = parents;
        }

        obj["geometry"] = new JSONArray();
        obj["geometry"].Add(GetGeometryNode()); //todo: replace with json node that represents a CityJson geometry object
        return obj;
    }

    public JSONNode GetGeometryNode()
    {
        var node = new JSONObject();
        node["type"] = "MultiSurface"; //todo support other types?
        node["lod"] = Lod;
        var boundaries = new JSONArray();
        for (int i = 0; i < Polygons.Length; i++)
        {
            var surfaceArray = new JSONArray(); //defines the entire surface with holes
            var boundaryArray = new JSONArray(); // defines a polygon (1st is surface, 2+ is holes in first surface)
            var offsetArray = CityJSONFormatter.AbsoluteBoundaries[Polygons[i]];
            for (int j = 0; j < offsetArray.Length; j++)
            {
                boundaryArray.Add(offsetArray[j]);
            }
            surfaceArray.Add(boundaryArray);
            boundaries.Add(surfaceArray);
        }
        node["boundaries"] = boundaries;

        return node;
    }

    protected virtual void Start()
    {
        Name = gameObject.name;
        Polygons = GetComponentsInChildren<CityPolygon>();
        CityJSONFormatter.AddCityObejct(this);
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            print(CityJSONFormatter.GetJSON());
        }
    }
}
