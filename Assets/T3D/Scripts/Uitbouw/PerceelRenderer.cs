using ConvertCoordinates;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Netherlands3D.Utilities;

namespace Netherlands3D.T3D.Uitbouw
{
    public class PerceelRenderer : MonoBehaviour
    {
        public Material LineMaterial;
        public Material PerceelMaterial;

        [SerializeField]
        private BuildingMeshGenerator building;

        [SerializeField]
        private GameObject perceelMeshGameObject;
        [SerializeField]
        private GameObject perceelOutlineGameObject;

        [SerializeField]
        private GameObject terreinMeshGameObject;

        public List<Vector2[]> Perceel { get; private set; }
        public float Area { get; private set; }
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }
        public bool IsLoaded { get; private set; } = false;
        public Plane PerceelPlane { get; private set; }

        private void Start()
        {
            //perceelMeshGameObject = CreatePerceelGameObject();
            //perceelOutlineGameObject = CreatePerceelGameObject();

            ServiceLocator.GetService<MetadataLoader>().PerceelDataLoaded += Instance_PerceelDataLoaded;
            building.BuildingDataProcessed += BuildingMeshGenerator_BuildingDataProcessed;
        }

        private void BuildingMeshGenerator_BuildingDataProcessed(BuildingMeshGenerator building)
        {
            perceelMeshGameObject.transform.position = new Vector3(perceelMeshGameObject.transform.position.x, building.GroundLevel, perceelMeshGameObject.transform.position.z);
            perceelOutlineGameObject.transform.position = new Vector3(perceelOutlineGameObject.transform.position.x, building.GroundLevel + 0.01f, perceelOutlineGameObject.transform.position.z);

            terreinMeshGameObject.transform.position = new Vector3(building.transform.position.x, building.GroundLevel, building.transform.position.z);
            Center = new Vector3(Center.x, building.GroundLevel, Center.z);
            PerceelPlane = new Plane(Vector3.up, building.GroundLevel);
        }

        private void Instance_PerceelDataLoaded(object source, PerceelDataEventArgs args)
        {
            Perceel = args.Perceel;
            GenerateMeshFromPerceel(args.Perceel);
            RenderPerceelOutline(args.Perceel);

            SetPerceelActive(false);
            SetTerrainActive(true);
            SetPerceelOutlineActive(true);

            Area = args.Area;
            var coord = CoordConvert.RDtoUnity(args.Center);
            Center = new Vector3(coord.x, Center.y, coord.z);
            Radius = args.Radius;
            IsLoaded = true;
        }

        void GenerateMeshFromPerceel(List<Vector2[]> perceel)
        {
            Mesh mesh = new Mesh();
            perceelMeshGameObject.name = "Perceelmesh";
            MeshFilter filter = perceelMeshGameObject.GetComponent<MeshFilter>();
            perceelMeshGameObject.GetComponent<MeshRenderer>().material = PerceelMaterial;

            var vertices = new List<Vector3>();
            var tris = new List<int[]>();

            for (int i = 0; i < perceel.Count; i++)
            {
                Vector2[] perceelPart = perceel[i];
                perceelPart = perceelPart.Take(perceelPart.Length - 1).ToArray(); //first and last point are the same, triangulation code cannot deal with that, so remove the last element

                var v3Points = from p in perceelPart
                               select CoordConvert.RDtoUnity(p) into v3
                               select new Vector3(v3.x, 0, v3.z);

                var perceelPartUnityCoordinates = from p in v3Points select new Vector2(p.x, p.z); //in a WebGL build, using perceelPart to triangulate causes problems, not 100% sure why but I suspect an overflow issue. This issue does not occur in the editor

                var subTris = GeometryCalculator.Triangulate(perceelPartUnityCoordinates.ToArray());

                vertices.AddRange(v3Points);
                tris.Add(subTris);
            }

            mesh.SetVertices(vertices);

            //set normals
            Vector3[] normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }
            mesh.normals = normals;

            for (int i = 0; i < perceel.Count; i++)
            {
                mesh.SetTriangles(tris[i], i);
            }

            mesh.RecalculateNormals();
            filter.mesh = mesh;
        }

        void RenderPerceelOutline(List<Vector2[]> perceel)
        {
            List<Vector2> vertices = new List<Vector2>();
            List<int> indices = new List<int>();

            int count = 0;
            foreach (var list in perceel)
            {
                for (int i = 0; i < list.Length - 1; i++)
                {
                    indices.Add(count + i);
                    indices.Add(count + i + 1);
                }
                count += list.Length;
                vertices.AddRange(list);
            }

            perceelOutlineGameObject.name = "PerceelOutline";
            MeshFilter filter = perceelOutlineGameObject.GetComponent<MeshFilter>();
            perceelOutlineGameObject.GetComponent<MeshRenderer>().material = LineMaterial;

            var verts = vertices.Select(o => CoordConvert.RDtoUnity(o)).ToArray();

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            filter.sharedMesh = mesh;
        }

        public void SetPerceelActive(bool active)
        {
            perceelMeshGameObject.SetActive(active);
            //perceelOutlineGameObject.SetActive(active);

            //terreinMeshGameObject.SetActive(!active);
            //perceelOutlineGameObject.SetActive(!active);
        }

        public void SetPerceelOutlineActive(bool active)
        {
            perceelOutlineGameObject.SetActive(active);
        }

        public void SetTerrainActive(bool active)
        {
            terreinMeshGameObject.SetActive(active);
        }
    }
}