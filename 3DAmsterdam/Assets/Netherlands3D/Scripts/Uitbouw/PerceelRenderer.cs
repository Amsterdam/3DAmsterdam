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
using Netherlands3D.Utilities;

namespace Netherlands3D.T3D.Uitbouw
{
    public class PerceelRenderer : MonoBehaviour
    {
        public Material LineMaterial;
        //public static PerceelRenderer Instance;
        //public GameObject TerrainLayer;
        //public GameObject BuildingsLayer;

        //public GameObject Uitbouw;
        

        [SerializeField]
        private BuildingMeshGenerator building;

        private GameObject perceelGameObject;

        public List<Vector2[]> Perceel { get; private set; }
        public float Area { get; private set; }
        //[SerializeField]
        //private Text MainTitle;

        //[SerializeField]
        //private Transform GeneratedFieldsContainer;

        private float terrainFloor;

        //private void awake()
        //{
        //    Instance = this;
        //}

        private void Start()
        {
            //PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
            //MainTitle.text = "Uitbouw plaatsen";

            //PropertiesPanel.Instance.AddSpacer();
            //PropertiesPanel.Instance.AddActionCheckbox("Toon alle gebouwen", true, (action) =>
            //{
            //    BuildingInterfaceLayer.ToggleLinkedObject(action);
            //    TerrainInterfaceLayer.ToggleLinkedObject(action);
            //});
            //PropertiesPanel.Instance.AddSpacer();

            MetadataLoader.Instance.PerceelDataLoaded += Instance_PerceelDataLoaded;
            building.BuildingDataProcessed += BuildingMeshGenerator_BuildingDataProcessed;
        }

        private void BuildingMeshGenerator_BuildingDataProcessed(BuildingMeshGenerator building)
        {
            perceelGameObject.transform.position = new Vector3(perceelGameObject.transform.position.x, building.GroundLevel, perceelGameObject.transform.position.z);
        }

        private void Instance_PerceelDataLoaded(object source, PerceelDataEventArgs args)
        {
            Perceel = args.Perceel;
            GenerateMeshFromPerceel(args.Perceel);

            Area = 0;
            foreach (var perceelPart in args.Perceel)
            {
                Area += GeometryCalculator.Area(perceelPart);
            }
            //RenderPolygons(args.Perceel);
        }

        //public void PlaatsUitbouw(Vector2RD rd)
        //{
        //    var pos = CoordConvert.RDtoUnity(rd);

        //    var uitbouw = Instantiate(Uitbouw, pos, Quaternion.identity);
        //}

        //public List<Vector3> vertices = new List<Vector3>();
        //public int[] tris0;

        void GenerateMeshFromPerceel(List<Vector2[]> perceel)
        {
            perceelGameObject = new GameObject();
            perceelGameObject.name = "Perceelmesh";
            perceelGameObject.transform.parent = transform;

            Mesh mesh = new Mesh();
            MeshFilter filter = perceelGameObject.AddComponent<MeshFilter>();
            perceelGameObject.AddComponent<MeshRenderer>().material = LineMaterial;

            var vertices = new List<Vector3>();
            var tris = new List<int[]>();

            for (int i = 0; i < perceel.Count; i++)
            {
                Vector2[] perceelPart = perceel[i];
                perceelPart = perceelPart.Take(perceelPart.Length - 1).ToArray(); //first and last point are the same, triangulation code cannot deal with that, so remove the last element

                var v3Points = from p in perceelPart
                               select CoordConvert.RDtoUnity(p) into v3
                               select new Vector3(v3.x, 0, v3.z);

                var subTris = GeometryCalculator.Triangulate(perceelPart);

                vertices.AddRange(v3Points);
                tris.Add(subTris);
            }

            mesh.SetVertices(vertices);

            for (int i = 0; i < perceel.Count; i++)
            {
                mesh.SetTriangles(tris[i], i);
            }
            filter.mesh = mesh;

            //var test = mesh.vertices;
            //for (int i = 0; i < test.Length - 1; i++)
            //{
            //    Debug.DrawLine(test[i], test[i + 1], Color.red, 1000);

            //    var g = new GameObject(i.ToString());
            //    g.transform.position = test[i];
            //}
            //Debug.DrawLine(test[0], test[test.Length - 1], Color.red, 1000);
            //var go = new GameObject((test.Length-1).ToString());
            //go.transform.position = test[test.Length - 1];
        }

        void RenderPolygons(List<Vector2[]> polygons)
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

            //while (!terrainFloorPoints.Any())
            //{
            //use collider to place the polygon points on the terrain
            for (int i = 0; i < verts.Length; i++)
            {
                var point = verts[i];
                var from = new Vector3(point.x, point.y + 10, point.z);


                Ray ray = new Ray(from, Vector3.down);
                RaycastHit hit;

                //yield return null;

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
                //}
                //yield return null;
            }

            terrainFloor = terrainFloorPoints.Average();

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            filter.sharedMesh = mesh;
        }
    }
}