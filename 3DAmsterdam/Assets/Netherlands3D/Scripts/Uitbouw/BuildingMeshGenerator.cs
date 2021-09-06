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
            print(vertIndices.Count);
            //copy mesh data to avoid getting a copy every iteration in the loop
            var sourceVerts = objectData.mesh.vertices;
            var trianglesLength = objectData.mesh.triangles.Length;
            var sourceTriangles = objectData.mesh.triangles;
            var sourceUVs = objectData.uvs;

            print(trianglesLength);
            for (int i = 0; i < trianglesLength; i += 3)
            {
                if (vertIndices.Contains(sourceTriangles[i]) || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                {
                    //add matching vert to my mesh
                    var vertStartIndex = sourceTriangles[i];

                    vertices.Add(sourceVerts[vertStartIndex]);
                    vertices.Add(sourceVerts[vertStartIndex + 1]);
                    vertices.Add(sourceVerts[vertStartIndex + 2]);

                    //add triangle to my own mesh
                    triIndices.Add(vertices.Count - 3);
                    triIndices.Add(vertices.Count - 2);
                    triIndices.Add(vertices.Count - 1);

                    uvs.Add(sourceUVs[vertStartIndex]);
                    uvs.Add(sourceUVs[vertStartIndex + 1]);
                    uvs.Add(sourceUVs[vertStartIndex + 2]);
                }
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
