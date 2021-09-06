using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;


namespace Netherlands3D.LayerSystem
{

    //public class BuildingMeshEventArgs : EventArgs
    //{
    //    public bool IsLoaded { get; private set; }
    //    public Mesh BuildingMesh { get; private set; }

    //    public BuildingMeshEventArgs(bool isLoaded, Mesh buildingMesh)
    //    {
    //        IsLoaded = isLoaded;
    //        BuildingMesh = buildingMesh;
    //    }
    //}

    public class BuildingMeshGenerator : MonoBehaviour
    {
        /*
        public bool IsLoaded { get; private set; }
        public Mesh BuildingMesh { get; private set; }
        
        public delegate void BuildingMeshLoadedEventHandler(object source, BuildingMeshEventArgs args);
        public event BuildingMeshLoadedEventHandler BuildingMeshLoaded;

        public static BuildingMeshGenerator Instance;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Debug.LogError("Instance of BuildingMeshGenerator already exists: " + Instance.gameObject.name, Instance.gameObject);
        }

        public void StartBuildingMeshRequest(Tile tile, string id)
        {
            StartCoroutine(RequestBuildingMesh(tile, id));
        }

        private IEnumerator RequestBuildingMesh(Tile tile, string id)
        {
            IsLoaded = false;
            Mesh tileMesh = tile.gameObject.GetComponent<MeshFilter>().mesh;
            string name = tileMesh.name;

            Debug.Log(name);
            string dataName = name.Replace(" Instance", "");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") + "-data";
            string dataURL = Config.activeConfiguration.buildingsMetaDataPath + dataName;

            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("UWR error", gameObject);
                }
                else
                {
                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    ObjectMappingClass data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];

                    List<int> vertIndices = new List<int>();
                    for (int i = 0; i < data.ids.Count; i++)
                    {
                        if (data.ids[i] == id)
                        {
                            vertIndices.Add(i);
                        }
                    }
                    BuildingMesh = ExtractBuildingMesh(tileMesh, vertIndices, data.uvs);
                    IsLoaded = true;
                    BuildingMeshLoaded?.Invoke(this, new BuildingMeshEventArgs(IsLoaded, BuildingMesh));
                }
            }
        }
        */

        public static List<int> triIndices = new List<int>();
        public static List<Vector3> vertices = new List<Vector3>();
        public static List<Vector2> uvs = new List<Vector2>();
        public static Vector3 offset;

        private void Start()//in start to avoid race conditions
        {
            PerceelRenderer.Instance.BuildingMetaDataLoaded += PerceelRenderer_BuildingMetaDataLoaded;
        }

        private void PerceelRenderer_BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
        {
            print("constructing mesh");
            //print(args.ObjectData.highlightIDs[0]);
            //print(args.ObjectData.ids.Count);
            //print(args.ObjectData.uvs.Length);

            offset = args.TileOffset;
            var buildingMesh = ExtractBuildingMesh(args.ObjectData, args.ObjectData.highlightIDs[0]);

            transform.position = args.TileOffset;
            var mf = GetComponent<MeshFilter>();
            mf.mesh = buildingMesh;
        }

        public static Mesh ExtractBuildingMesh(ObjectData objectData, string id)
        {
            var idIndex = objectData.ids.IndexOf(id);

            //var idIndex = 0;
            List<int> vertIndices = new List<int>();
            for (int i = 0; i < objectData.vectorMap.Count; i++)
            {
                if (objectData.vectorMap[i] == idIndex)
                {
                    vertIndices.Add(i);
                }
            }

            //copy mesh data to avoid getting a copy every iteration in the loop
            var sourceVerts = objectData.mesh.vertices;
            var sourceTriangles = objectData.mesh.triangles;
            var sourceUVs = objectData.uvs;

            for (int i = 0; i < sourceTriangles.Length; i += 3)
            {
                var triStartIndex = sourceTriangles[i];

                if (vertIndices.Contains(triStartIndex))// || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                {
                    //add matching vert to my mesh
                    print("test " + 1);
                    vertices.Add(sourceVerts[triStartIndex]);
                    vertices.Add(sourceVerts[triStartIndex + 1]);
                    vertices.Add(sourceVerts[triStartIndex + 2]);

                    //add triangle to my own mesh

                    //determine order of triIndex
                    for (int j = -2; j < 2; j++)
                    {
                        if (vertIndices.Contains(sourceTriangles[i + j]))
                        {

                        }
                    }

                    triIndices.Add(vertices.Count - 3);
                    triIndices.Add(vertices.Count - 2);
                    triIndices.Add(vertices.Count - 1);

                    triIndices.Add(vertices.Count - 1);
                    triIndices.Add(vertices.Count - 2);
                    triIndices.Add(vertices.Count - 3);

                    //add uvs
                    uvs.Add(sourceUVs[triStartIndex]);
                    uvs.Add(sourceUVs[triStartIndex + 1]);
                    uvs.Add(sourceUVs[triStartIndex + 2]);
                }

                //if (vertIndices.Contains(triStartIndex + 1))// || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                //{
                //    //add matching vert to my mesh

                //    print("test" + 2);
                //    vertices.Add(sourceVerts[triStartIndex]);
                //    vertices.Add(sourceVerts[triStartIndex + 1]);
                //    vertices.Add(sourceVerts[triStartIndex + 2]);

                //    //add triangle to my own mesh
                //    triIndices.Add(vertices.Count - 3);
                //    triIndices.Add(vertices.Count - 2);
                //    triIndices.Add(vertices.Count - 1);

                //    //add uvs
                //    uvs.Add(sourceUVs[triStartIndex]);
                //    uvs.Add(sourceUVs[triStartIndex + 1]);
                //    uvs.Add(sourceUVs[triStartIndex + 2]);
                //}

                //if (vertIndices.Contains(triStartIndex + 2))// || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                //{
                //    //add matching vert to my mesh
                //    print("test" + 3);

                //    vertices.Add(sourceVerts[triStartIndex]);
                //    vertices.Add(sourceVerts[triStartIndex + 1]);
                //    vertices.Add(sourceVerts[triStartIndex + 2]);

                //    //add triangle to my own mesh
                //    triIndices.Add(vertices.Count - 3);
                //    triIndices.Add(vertices.Count - 2);
                //    triIndices.Add(vertices.Count - 1);

                //    //add uvs
                //    uvs.Add(sourceUVs[triStartIndex]);
                //    uvs.Add(sourceUVs[triStartIndex + 1]);
                //    uvs.Add(sourceUVs[triStartIndex + 2]);
                //}
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triIndices.ToArray();
            mesh.uv = uvs.ToArray();
            return mesh;
        }

        private void OnDrawGizmos()
        {
            foreach (var vert in vertices)
            {
                Debug.DrawLine(Vector3.zero, vert + offset, Color.red);
                Gizmos.DrawCube(vert, Vector3.one);
            }
        }
    }
}
