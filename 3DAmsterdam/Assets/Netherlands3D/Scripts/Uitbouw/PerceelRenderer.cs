using ConvertCoordinates;
using Netherlands3D.Interface.SidePanel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Netherlands3D.Interface.Layers;
using Netherlands3D.Utilities;

namespace Netherlands3D.T3D.Uitbouw
{
    public class PerceelRenderer : MonoBehaviour
    {
        public Material LineMaterial;
        public Material PerceelMaterial;
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
            CreatePerceelGameObject();

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

            Area = args.Area;
        }

        void GenerateMeshFromPerceel(List<Vector2[]> perceel)
        {
            Mesh mesh = new Mesh();
            MeshFilter filter = perceelGameObject.AddComponent<MeshFilter>();
            perceelGameObject.GetComponent<MeshRenderer>().material = PerceelMaterial;

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

            //set normals
            Vector3[] normals = new Vector3[vertices.Count];
            for(int i=0; i< normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }
            mesh.normals = normals;

            for (int i = 0; i < perceel.Count; i++)
            {
                mesh.SetTriangles(tris[i], i);
            }
            filter.mesh = mesh;
        }

        private void CreatePerceelGameObject()
        {
            perceelGameObject = new GameObject();
            perceelGameObject.name = "Perceelmesh";
            perceelGameObject.transform.parent = transform;
            perceelGameObject.AddComponent<MeshRenderer>();
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