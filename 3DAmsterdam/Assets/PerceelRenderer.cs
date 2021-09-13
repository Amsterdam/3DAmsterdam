using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.LayerSystem;
using System;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using Netherlands3D.Interface.Layers;

public class PerceelRenderer : MonoBehaviour
{
    public Material LineMaterial;
    //public static PerceelRenderer Instance;
    //public GameObject TerrainLayer;
    //public GameObject BuildingsLayer;

    //public GameObject Uitbouw;
    public InterfaceLayer BuildingInterfaceLayer;

    //[SerializeField]
    //private Text MainTitle;

    //[SerializeField]
    //private Transform GeneratedFieldsContainer;

    private float terrainFloor;

    //private void Awake()
    //{
    //    Instance = this;
    //}

    private void Start()
    {
        //PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
        //MainTitle.text = "Uitbouw plaatsen";

        PropertiesPanel.Instance.AddSpacer();
        PropertiesPanel.Instance.AddActionCheckbox("Toon alle gebouwen", true, (action) =>
        {
            BuildingInterfaceLayer.ToggleLinkedObject(action);
        });
        PropertiesPanel.Instance.AddSpacer();

        MetadataLoader.Instance.PerceelDataLoaded += Instance_PerceelDataLoaded;
    }

    private void Instance_PerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        StartCoroutine(RenderPolygons(args.Perceel));
    }

    //public void PlaatsUitbouw(Vector2RD rd)
    //{
    //    var pos = CoordConvert.RDtoUnity(rd);

    //    var uitbouw = Instantiate(Uitbouw, pos, Quaternion.identity);
    //}

    IEnumerator RenderPolygons(List<Vector2[]> polygons)
    {

        List<Vector2> vertices = new List<Vector2>();
        List<int> indices = new List<int>();

        int count = 0;
        foreach (var list in polygons)
        {
            for (int i = 0; i < list.Length - 1; i++)
            {
                indices.Add(count + i);
                indices.Add(count + i + 1);
            }
            count += list.Length;
            vertices.AddRange(list);
        }

        GameObject gam = new GameObject();
        gam.name = "Perceel";
        gam.transform.parent = transform;

        MeshFilter filter = gam.AddComponent<MeshFilter>();
        gam.AddComponent<MeshRenderer>().material = LineMaterial;

        var verts = vertices.Select(o => CoordConvert.RDtoUnity(o)).ToArray();

        List<float> terrainFloorPoints = new List<float>();

        while (!terrainFloorPoints.Any())
        {
            //use collider to place the polygon points on the terrain
            for (int i = 0; i < verts.Length; i++)
            {
                var point = verts[i];
                var from = new Vector3(point.x, point.y + 10, point.z);


                Ray ray = new Ray(from, Vector3.down);
                RaycastHit hit;

                yield return null;

                //raycast from the polygon point to hit the terrain so we can place the outline so that it is visible
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    //Debug.Log("we have a hit");
                    verts[i].y = hit.point.y + 0.5f;
                    terrainFloorPoints.Add(hit.point.y);
                }
                else
                {
                    Debug.Log("raycast failed..");
                }
            }
            yield return null;
        }

        terrainFloor = terrainFloorPoints.Average();

        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.sharedMesh = mesh;
    }

}
