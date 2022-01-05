#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Netherlands3D.Core;
using SimpleJSON;
using System.Threading;
using System.Linq;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public class ImportCityJsonTerrain : MonoBehaviour
    {
        public List<string> missedTiles = new List<string>();

        private List<Material> materialList = new List<Material>(7);
        private Material[] materialsArray;
        private List<Vector3RD> vertsRD = new List<Vector3RD>();
        private List<Vector3> defshape;
        public float heightMax;
        public float heightMin;
        // Start is called before the first frame update
        void Start()
        {
            //materialsArray = materialList.ToArray();
            ////double originX = 121000;
            ////double originY = 487000;
            ////ImportSingle(originX, originY);
            //Import();
        }

        public void CreateCombinedMeshes(ref List<Mesh> meshes,Vector2 tileID, float tileSize)
        {
            int vertcount = 0;
            CombineInstance[] combi = new CombineInstance[meshes.Count];
            for (int i = 0; i < combi.Length; i++)
            {
                combi[i].mesh = meshes[i];
                vertcount += meshes[i].vertexCount;
            }

            Mesh lod1Mesh = new Mesh();
                lod1Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            lod1Mesh.CombineMeshes(combi, false, false);

           
            //remove spikes
            RemoveSpikes(ref lod1Mesh);
            lod1Mesh.Optimize();
            string assetName = "Assets/GeneratedTileAssets/terrain_" + (int)tileID.x + "-" + (int)tileID.y + "-lod1.mesh";


            Mesh existingMesh = (Mesh)AssetDatabase.LoadAssetAtPath(assetName, typeof(Mesh));

            if (existingMesh != null)
            {
                //combine meshes;
                CombineMeshes(ref lod1Mesh, ref existingMesh);
                DestroyImmediate(existingMesh, true);
            }
            lod1Mesh.uv2 = RDuv2(lod1Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(tileID.x, tileID.y, 0)), tileSize);
            lod1Mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(lod1Mesh, assetName);
           AssetDatabase.SaveAssets();
            Resources.UnloadAsset(lod1Mesh);

           

            // create lod0-mesh
            vertcount = 0;
            //simplify all submeshes
            Mesh tempMesh;
            for (int i = 0; i < combi.Length; i++)
            {
                tempMesh = combi[i].mesh;
                SimplifyMesh(ref tempMesh, 0.05f);
                combi[i].mesh = tempMesh;
                vertcount += combi[i].mesh.vertexCount;
            }

            Mesh lod0Mesh = new Mesh();

                lod0Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            lod0Mesh.CombineMeshes(combi, false, false);

            RemoveSpikes(ref lod0Mesh);
            
            string assetNameLod0 = "Assets/GeneratedTileAssets/terrain_" + (int)tileID.x + "-" + (int)tileID.y + "-lod0.mesh";
            lod0Mesh.Optimize();
            existingMesh = (Mesh)AssetDatabase.LoadAssetAtPath(assetNameLod0, typeof(Mesh));
            if (existingMesh != null)
            {
                //combine meshes;
                CombineMeshes(ref lod0Mesh, ref existingMesh);
                DestroyImmediate(existingMesh, true);

            }
            lod0Mesh.uv2 = RDuv2(lod0Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(tileID.x, tileID.y, 0)), tileSize);
            lod0Mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(lod0Mesh, assetNameLod0);
            AssetDatabase.SaveAssets();
            Resources.UnloadAsset(lod0Mesh);


            //cleanup
            for (int i = 0; i < combi.Length; i++)
            {
                Destroy(combi[i].mesh);
            }
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
            //DestroyImmediate(lod1Mesh);
            //DestroyImmediate(lod0Mesh);
            //for (int i = 0; i < 20; i++) { System.GC.Collect(); }
        }



        private void RemoveSpikes(ref Mesh mesh)
        {
            if (mesh.vertices.Length == 0)
            {
                return;
            };

            var verts = mesh.vertices;

            var correctverts = verts.Where(o => o.y < heightMax && o.y > heightMin);

            var correctvertsAvgHeight = correctverts.Average(o => o.y);

            bool hasspike = false;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].y > heightMax || verts[i].y < heightMin)
                {
                    hasspike = true;
                    verts[i].y = correctvertsAvgHeight; //for now just use the average height

                    //experimental code, needs further testing
                    //var x = verts[i].x;
                    //var z = verts[i].z;
                    //var vertsaround = correctverts.Where(o => o.x < x + lookaroundWidth
                    //								&& o.x > x - lookaroundWidth
                    //								&& o.z < z + lookaroundWidth
                    //								&& o.z > z - lookaroundWidth);
                    //if (vertsaround.Any())
                    //{
                    //	var avgh = vertsaround.Max(o => o.y);
                    //	verts[i].y = avgh;
                    //}
                    //else
                    //{
                    //	verts[i].y = correctvertsAvgHeight;
                    //}
                }
            }

            if (hasspike) mesh.vertices = verts;
            verts = null;
            
        }

        private void CombineMeshes(ref Mesh mesh1, ref Mesh mesh2)
        {

            
            if (mesh2.subMeshCount<2)
            {
                missedTiles.Add(mesh2.name+"-combining");

                ////newMesh = copyMesh(ref mesh1);
                //DestroyImmediate(mesh1,true);
                return;
            }
            //newMesh.
            
            int meshVertexCount = mesh1.vertexCount;
            List<Vector3> verts = new List<Vector3>(mesh1.vertices);
            verts.AddRange(mesh2.vertices);
            List<Vector2> newUVs = new List<Vector2>(mesh1.uv2);
            newUVs.AddRange(new List<Vector2>(mesh2.uv2));

            //if (verts.Count > 65500 && mesh1.indexFormat== UnityEngine.Rendering.IndexFormat.UInt16)
            //{
            //    Mesh newMesh = new Mesh();
            //    newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    newMesh.vertices = mesh1.vertices;
            //    newMesh.subMeshCount = mesh1.subMeshCount;
            //    for (int i = 0; i < mesh1.subMeshCount; i++)
            //    {
            //        newMesh.SetIndices(mesh1.GetIndices(i), MeshTopology.Triangles, i);
            //    }
            //    mesh1.subMeshCount = newMesh.subMeshCount;
            //    mesh1.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    for (int i = 0; i < mesh1.subMeshCount; i++)
            //    {
            //        mesh1.SetIndices(newMesh.GetIndices(i), MeshTopology.Triangles, i);
            //    }
            //    DestroyImmediate(newMesh,true);
            //}
            mesh1.vertices = verts.ToArray();
            //mesh1.uv2 = newUVs.ToArray();
           
            
            for (int submeshIndex = 0; submeshIndex < mesh1.subMeshCount; submeshIndex++)
            {
                // = new List<int>();
                List<int> submeshIndices = new List<int>(mesh1.GetIndices(submeshIndex));

                List<int> extraIndices = new List<int>(mesh2.GetIndices(submeshIndex));

                for (int i = 0; i < extraIndices.Count; i++)
                {
                    extraIndices[i] += meshVertexCount;
                }
                submeshIndices.AddRange(extraIndices);
                mesh1.SetIndices(submeshIndices, MeshTopology.Triangles, submeshIndex);
            }

            mesh1.RecalculateNormals();
            verts.Clear();
            newUVs.Clear();
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
            
            
            }
        public void SimplifyMesh(ref Mesh mesh, float quality)
        {

            if (mesh.triangles.Length < 100)
            {
                // mesh = copyMesh(ref mesh);
                return;
            }

            //var DecimatedMesh = mesh;

            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.Initialize(mesh);
            meshSimplifier.PreserveBorderEdges = true;
            meshSimplifier.MaxIterationCount = 500;
            meshSimplifier.EnableSmartLink = true;
            DestroyImmediate(mesh, true);
            meshSimplifier.SimplifyMesh(quality);
            mesh = meshSimplifier.ToMesh();
            //mesh.SetIndices(meshSimplifier.GetSubMeshTriangles(0), MeshTopology.Triangles, 0);
            //mesh.vertices = meshSimplifier.Vertices;

            meshSimplifier = null;
            mesh.RecalculateNormals();
            mesh.Optimize();
            
        }


        private bool PointISInsideArea(Vector3RD point, double OriginX, double OriginY, float tileSize)
        {

            if (point.x < OriginX || point.x > (OriginX + tileSize))
            {
                return false;
            }
            if (point.y < OriginY || point.y > (OriginY + tileSize))
            {
                return false;
            }

            return true;
        }

        public Mesh CreateCityObjectMesh(List<Vector3RD> RDTriangles, double originX, double originY, float tileSize)
        {

            List<Vector3RD> clippedRDTriangles = new List<Vector3RD>();
            clippedRDTriangles.Capacity = RDTriangles.Count;
            List<Vector3> vectors = new List<Vector3>();
            vectors.Capacity = RDTriangles.Count;
            List<Vector3> clipboundary = CreateClippingPolygon(tileSize);

            if (RDTriangles.Count == 0)
            {
                //Debug.Log("no triangles in mesh");
                return CreateEmptyMesh();
            }

            //clip all the triangles
            for (int i = 0; i < RDTriangles.Count; i += 3)
            {

                if (PointISInsideArea(RDTriangles[i], originX, originY, tileSize) && PointISInsideArea(RDTriangles[i + 1], originX, originY, tileSize) && PointISInsideArea(RDTriangles[i + 2], originX, originY, tileSize))
                {
                    clippedRDTriangles.Add(RDTriangles[i + 2]);
                    clippedRDTriangles.Add(RDTriangles[i + 1]);
                    clippedRDTriangles.Add(RDTriangles[i]);
                    continue;
                }


                //offset RDvertices so coordinates can be saved as a float
                // flip y and z-axis so clippingtool works
                //reverse order to make them clockwise so the clipping-algorithm can use them
                vectors.Clear();
                vectors.Add(new Vector3((float)(RDTriangles[i + 2].x - originX), (float)RDTriangles[i + 2].z, (float)(RDTriangles[i + 2].y - originY)));
                vectors.Add(new Vector3((float)(RDTriangles[i + 1].x - originX), (float)RDTriangles[i + 1].z, (float)(RDTriangles[i + 1].y - originY)));
                vectors.Add(new Vector3((float)(RDTriangles[i].x - originX), (float)RDTriangles[i].z, (float)(RDTriangles[i].y - originY)));
                vectors.Reverse();
                defshape = Netherlands3D.Utilities.TriangleClipping.SutherlandHodgman.ClipPolygon(vectors, clipboundary);

                if (defshape.Count < 3)
                {
                    continue;
                }

                if (defshape[0].x.ToString() == "NaN")
                {
                    continue;
                }

                defshape.Reverse();

                Vector3RD vectorRD = new Vector3RD();
                // add first three vectors

                vectorRD.x = defshape[0].x + originX;
                vectorRD.y = defshape[0].z + originY;
                vectorRD.z = defshape[0].y;
                clippedRDTriangles.Add(vectorRD);

                vectorRD.x = defshape[1].x + originX;
                vectorRD.y = defshape[1].z + originY;
                vectorRD.z = defshape[1].y;
                clippedRDTriangles.Add(vectorRD);
                vectorRD.x = defshape[2].x + originX;
                vectorRD.y = defshape[2].z + originY;
                vectorRD.z = defshape[2].y;
                clippedRDTriangles.Add(vectorRD);

                // add extra vectors. vector makes a triangle with the first and the previous vector.
                for (int j = 3; j < defshape.Count; j++)
                {
                    vectorRD.x = defshape[0].x + originX;
                    vectorRD.y = defshape[0].z + originY;
                    vectorRD.z = defshape[0].y;
                    clippedRDTriangles.Add(vectorRD);

                    vectorRD.x = defshape[j - 1].x + originX;
                    vectorRD.y = defshape[j - 1].z + originY;
                    vectorRD.z = defshape[j - 1].y;
                    clippedRDTriangles.Add(vectorRD);

                    vectorRD.x = defshape[j].x + originX;
                    vectorRD.y = defshape[j].z + originY;
                    vectorRD.z = defshape[j].y;
                    clippedRDTriangles.Add(vectorRD);
                }
                defshape.Clear();
            }

            //createMesh
            List<Vector3> verts = new List<Vector3>();
            verts.Capacity = clippedRDTriangles.Count();
            Vector3RD tileCenterRD = new Vector3RD();
            tileCenterRD.x = originX + (tileSize / 2);
            tileCenterRD.y = originY + (tileSize / 2);
            tileCenterRD.z = 0;
            List<int> ints = new List<int>();
            ints.Capacity = clippedRDTriangles.Count();
            for (int i = 0; i < clippedRDTriangles.Count; i++)
            {
                Vector3 coord = new Vector3(
                    (float)(clippedRDTriangles[i].x-tileCenterRD.x),
                    (float)clippedRDTriangles[i].z,
                    (float)(clippedRDTriangles[i].y - tileCenterRD.y)
                    

                    );
                ints.Add(i);
                verts.Add(coord);
            }
            ints.Reverse(); //reverse the trianglelist to make the triangles counter-clockwise again

            if (ints.Count == 0)
            {
               // Debug.Log("no ints");
                return CreateEmptyMesh();
            }
            Mesh mesh = new Mesh();
            if (verts.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.vertices = verts.ToArray();
            mesh.triangles = ints.ToArray();
            WeldVertices(ref mesh);
            mesh.RecalculateNormals();
            mesh.Optimize();
            clippedRDTriangles.Clear();
            vectors.Clear();
            
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
            return mesh;
        }

        private Mesh CreateEmptyMesh()
        {
            Mesh emptyMesh = new Mesh();
            Vector3[] emptyVertsList = new Vector3[3];
            emptyVertsList[0] = new Vector3(0, 0, 0);
            List<int> emptyIndices = new List<int>();
            emptyIndices.Add(0);
            emptyIndices.Add(0);
            emptyIndices.Add(0);
            emptyMesh.vertices = emptyVertsList;
            emptyMesh.SetIndices(emptyIndices.ToArray(), MeshTopology.Triangles, 0);
            emptyIndices = new List<int>();
            return emptyMesh;
        }

        private Vector2[] RDuv2(Vector3[] verts, Vector3 UnityOrigin, float tileSize)
        {
            Vector3 UnityCoordinate;
            Vector3RD rdCoordinate;
            Vector3RD rdOrigin = CoordConvert.UnitytoRD(UnityOrigin);
            float offset = -tileSize / 2;
            Vector2[] uv2 = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                UnityCoordinate = verts[i] + UnityOrigin;
                rdCoordinate = CoordConvert.UnitytoRD(UnityCoordinate);
                uv2[i].x = ((float)(rdCoordinate.x - rdOrigin.x) + offset) / tileSize;
                uv2[i].y = ((float)(rdCoordinate.y - rdOrigin.y) + offset) / tileSize;
            }
            return uv2;
        }

        private void WeldVertices(ref Mesh mesh)
        {
            //Mesh weldedMesh = new Mesh();
            Vector3[] originalVerts = mesh.vertices;
            int[] originlints = mesh.GetIndices(0);
            int[] newIndices = new int[originlints.Length];
            Dictionary<Vector3, int> vertexMapping = new Dictionary<Vector3, int>();
            
            //fill the dictionary
            foreach (Vector3 vert in originalVerts)
            {
                if (!vertexMapping.ContainsKey(vert))
                {
                    vertexMapping.Add(vert, vertexMapping.Count);
                }
            }
            for (int i = 0; i < originlints.Length; i++)
            {
                newIndices[i] = vertexMapping[originalVerts[originlints[i]]];
            }
            mesh.Clear();
            if (vertexMapping.Count>65500)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = new List<Vector3>(vertexMapping.Keys).ToArray();
            mesh.triangles = newIndices;
            
        }


        









        private List<Vector3RD> GetVertsRD(CityModel cityModel)
        {
            List<Vector3RD> vertsRD = new List<Vector3RD>();
            Vector3 vertexCoordinate = new Vector3();
            foreach (Vector3Double vertex in cityModel.vertices)
            {
                vertexCoordinate.x = (float)vertex.x;
                vertexCoordinate.y = (float)vertex.z;
                vertexCoordinate.z = (float)vertex.y;
                vertsRD.Add(CoordConvert.UnitytoRD(vertexCoordinate));

            }
            return vertsRD;
        }
        public List<Vector3RD> GetTriangleListRD(CityModel cityModel, string cityObjectType, string bgtProperty, List<string> bgtValues, bool include)
        {
            
            List<Vector3RD> triangleList = new List<Vector3RD>();
            List<int> triangles = new List<int>();
            bool Include;
            foreach (JSONNode cityObject in cityModel.cityjsonNode["CityObjects"])
            {
                Include = !include;
                if (cityObject["type"] == cityObjectType)
                {
                    if (bgtValues == null)
                    {
                        Include = !Include;
                    }
                    else if (bgtValues.Contains(cityObject["attributes"][bgtProperty]))
                    {
                        Include = !Include;
                    }
                    if (Include)
                    {
                        triangles.AddRange(ReadTriangles(cityObject));
                    }


                }
            }
            for (int i = 0; i < triangles.Count; i++)
            {
                triangleList.Add(vertsRD[triangles[i]]);
            }
            return triangleList;
        }

        private List<Vector3> CreateClippingPolygon(float tilesize)
        {
            List<Vector3> polygon = new List<Vector3>();
            polygon.Add(new Vector3(0, 0, 0));
            polygon.Add(new Vector3(tilesize, 0, 0));
            polygon.Add(new Vector3(tilesize, 0, tilesize));
            polygon.Add(new Vector3(0, 0, tilesize));
            return polygon;
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
        string CreateAssetFolder(string folderpath, string foldername)
        {

            if (!AssetDatabase.IsValidFolder(folderpath + "/" + foldername))
            {
                AssetDatabase.CreateFolder(folderpath, foldername);
            }
            return folderpath + "/" + foldername;
        }
    }
}
#endif