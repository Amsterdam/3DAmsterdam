#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using SimpleJSON;

namespace Netherlands3D.AssetGeneration.CityJSON
{

    public class CreateTerrainSurface : MonoBehaviour
    {
        public List<string> fysiek_voorkomen = new List<string>();
        private List<Vector3> verts = new List<Vector3>();
        private Dictionary<string, List<int>> triangleLists;
        public GameObject CreateMesh(CityModel cityModel, Vector3RD origin)
        {
            GameObject container = new GameObject();

            //Terraindata terrainData = container.AddComponent<Terraindata>();

            verts = GetVerts(cityModel, origin);

            triangleLists = GetTriangleLists(cityModel);

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts.ToArray());
            mesh.subMeshCount = triangleLists.Count;

            int submeshnumber = 0;
            foreach (var item in triangleLists)
            {
                //terrainData.terrainTypes.Add(item.Key);
                //if (submeshnumber==0 || submeshnumber==1)
                //{
                    mesh.SetTriangles(item.Value.ToArray(), submeshnumber);
                    
                //}
                submeshnumber++;
            }
            
            mesh.RecalculateNormals();
            mesh.Optimize();
            container.AddComponent<MeshFilter>().mesh = mesh;
            container.AddComponent<MeshRenderer>().sharedMaterials = new Material[submeshnumber];

            return container;
        }

        private List<Vector3> GetVerts(CityModel cityModel, Vector3RD origin)
        {
            List<Vector3> verts = new List<Vector3>();
            Vector3 unityOrigin = CoordConvert.RDtoUnity(origin);
            Vector3RD vertexCoordinate = new Vector3RD();
            foreach (Vector3Double vertex in cityModel.vertices)
            {
                vertexCoordinate.x = vertex.x;
                vertexCoordinate.y = vertex.y;
                vertexCoordinate.z = vertex.z;
                verts.Add(CoordConvert.RDtoUnity(vertexCoordinate) - unityOrigin);
            }

            return verts;
        }

        private Dictionary<string, List<int>> GetTriangleLists(CityModel cityModel) 
            {
            Dictionary<string, List<int>> triangleList = new Dictionary<string, List<int>>();

            triangleList.Add("LandUse", new List<int>());
            triangleList.Add("PlantCover", new List<int>());
            triangleList.Add("Bridge", new List<int>());
            triangleList.Add("Road", new List<int>());
            triangleList.Add("Railway", new List<int>());
            triangleList.Add("TransportSquare", new List<int>());
            triangleList.Add("WaterBody", new List<int>());
            triangleList.Add("GenericCityObject", new List<int>());
            triangleList.Add("Building", new List<int>());


            string cityObjectType = "";
            foreach (JSONNode cityObject in cityModel.cityjsonNode["CityObjects"])
            {
                cityObjectType = cityObject["type"];

                // Skip Buildings
                //if (cityObjectType =="Building")
                //{
                //    continue;
                //}
                // Skip Bridges
                //if (cityObjectType == "Bridge")
                //{
                //    continue;
                //}

                if (cityObjectType == "LandUse")
                {
                    string property = cityObject["attributes"]["bgt_fysiekvoorkomen"].Value;
                    property = cityObject["attributes"]["bgt_functie"].Value;
                    if (fysiek_voorkomen.Contains(property) == false)
                    {
                        fysiek_voorkomen.Add(property);
                    }
                }

                if (triangleList.ContainsKey(cityObjectType)==true)
                {
                    triangleList[cityObjectType].AddRange(ReadTriangles(cityObject));
                }
                else
                {
                    Debug.Log("cityObject: " + cityObjectType);
                }
                
                
            }

            return triangleList;
            }

        private List<int> ReadTriangles(JSONNode cityObject)
        {
            List<int> triangles = new List<int>();
            JSONNode boundariesNode = cityObject["geometry"][0]["boundaries"];
            // End if no BoundariesNode
            if (boundariesNode is null)
            {
                return triangles;
            }
            foreach (JSONNode boundary in boundariesNode)
            {
                JSONNode outerRing = boundary[0];
                triangles.Add(outerRing[2].AsInt);
                triangles.Add(outerRing[1].AsInt);
                triangles.Add(outerRing[0].AsInt);
            }

            return triangles;
        }

    }

}
#endif