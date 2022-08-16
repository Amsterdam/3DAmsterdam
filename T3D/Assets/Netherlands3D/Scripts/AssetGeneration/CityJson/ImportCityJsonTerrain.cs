#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ConvertCoordinates;
using SimpleJSON;
using System.Threading;
using System.Linq;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public class ImportCityJsonTerrain : MonoBehaviour
    {
        private List<Material> materialList = new List<Material>(7);
        private Material[] materialsArray;
        private List<Vector3RD> vertsRD = new List<Vector3RD>();

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

        void ImportSingle(double OriginX, double OriginY)
        {
            double originX = OriginX;
            double originY = OriginY;
            string basefilepath = "E:/TiledData/Terrain1000x1000/";

            string jsonfilename = originX.ToString() + "-" + originY.ToString() + ".json";

            //float tileSize = 1000;
            string filepath = basefilepath;
            Debug.Log(filepath);
           
            //if (File.Exists(filepath + jsonfilename))
            //{
            //    CityModel cm = new CityModel(filepath, jsonfilename);
            //    vertsRD = GetVertsRD(cm);
            //    //type voetpad
            //    Mesh RoadsvoetpadMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "voetpad", "voetgangersgebied", "ruiterpad", "voetpad op trap" }, true);
            //    Mesh LandUseVoetpadMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "open verharding" }, true);
            //    LandUseVoetpadMesh = SimplifyMesh(LandUseVoetpadMesh, 0.5f);
            //    //combine meshes of type "voetpad"
            //    CombineInstance[] voetpadcombi = new CombineInstance[2];
            //    voetpadcombi[0].mesh = RoadsvoetpadMesh;
            //    voetpadcombi[1].mesh = LandUseVoetpadMesh;
            //    Mesh voetpadmesh = new Mesh();
            //    voetpadmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    voetpadmesh.CombineMeshes(voetpadcombi, true, false);


            //    //type fietspad
            //    Mesh fietspadMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "fietspad" }, true);

            //    //type parkeervak
            //    Mesh parkeervlakMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "parkeervlak" }, true);
            //    //type spoorbaan
            //    Mesh spoorbaanMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "spoorbaan" }, true);
            //    //type woonerf
            //    Mesh WoonerfMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "transitie", "woonerf" }, true);

            //    // type weg
            //    Mesh roadsMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "fietspad", "parkeervlak", "ruiterpad", "spoorbaan", "voetgangersgebied", "voetpad", "voetpad op trap", "woonerf" }, false);
            //    Mesh LandUseVerhardMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "gesloten verharding" }, true);
            //    LandUseVerhardMesh = SimplifyMesh(LandUseVerhardMesh, 0.5f);
            //    // combine meshes of type "weg"
            //    CombineInstance[] wegcombi = new CombineInstance[2];
            //    wegcombi[0].mesh = roadsMesh;
            //    wegcombi[1].mesh = LandUseVerhardMesh;
            //    Mesh wegmesh = new Mesh();
            //    wegmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    wegmesh.CombineMeshes(wegcombi, true, false);

            //    // type groen
            //    Mesh plantcoverMesh = CreateCityObjectMesh(cm, "PlantCover", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "alles" }, false);
            //    plantcoverMesh = SimplifyMesh(plantcoverMesh, 0.5f);
            //    Mesh LanduseGroenMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "groenvoorziening" }, true);
            //    LanduseGroenMesh = SimplifyMesh(LanduseGroenMesh, 0.5f);
            //    //combine meshes of type "groen"
            //    CombineInstance[] groencombi = new CombineInstance[2];
            //    groencombi[0].mesh = plantcoverMesh;
            //    groencombi[1].mesh = LanduseGroenMesh;
            //    Mesh groenMesh = new Mesh();
            //    groenMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    groenMesh.CombineMeshes(groencombi, true, false);

            //    //type erf
            //    Mesh erfMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "erf" }, true);
            //    erfMesh = SimplifyMesh(erfMesh, 0.5f);

            //    //type onverhard
            //    Mesh LandUseMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "erf", "groenvoorziening", "gesloten verharding", "open verharding" }, false);
            //    LandUseMesh = SimplifyMesh(LandUseMesh, 0.5f);

            //    Mesh genericCityObjectMesh = CreateCityObjectMesh(cm, "GenericCityObject", originX, originY, tileSize, "bgt_type", null, true);
            //    Mesh waterBodyMesh = CreateCityObjectMesh(cm, "WaterBody", originX, originY, tileSize, "bgt_type", null, true);
            //    Mesh bridgeMesh = CreateCityObjectMesh(cm, "Bridge", originX, originY, tileSize, "bgt_type", null, true);

            //    //create LOD1 Mesh
            //    CombineInstance[] combi = new CombineInstance[12];
            //    combi[0].mesh = voetpadmesh; //
            //    combi[1].mesh = fietspadMesh; //
            //    combi[2].mesh = parkeervlakMesh; //
            //    combi[3].mesh = wegmesh; //
            //    combi[4].mesh = groenMesh; //
            //    combi[5].mesh = erfMesh; //
            //    combi[6].mesh = LandUseMesh; //
            //    combi[7].mesh = spoorbaanMesh; //
            //    combi[8].mesh = WoonerfMesh; //
            //    combi[9].mesh = genericCityObjectMesh;
            //    combi[10].mesh = bridgeMesh;
            //    combi[11].mesh = waterBodyMesh;

            //    Mesh lod1Mesh = new Mesh();
            //    lod1Mesh.CombineMeshes(combi, false, false);
            //    lod1Mesh.uv2 = RDuv2(lod1Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(originX, originY, 0)), tileSize);
            //    //Physics.BakeMesh(lod1Mesh.GetInstanceID(), false);
            //    AssetDatabase.CreateAsset(lod1Mesh, "Assets/GeneratedTileAssets/terrain_" + originX + "-" + originY + "-lod1.mesh");
            //    //for debug

            //    //GetComponent<MeshFilter>().sharedMesh = lod1Mesh;

            //    //create LOD0MEsh
            //    combi = new CombineInstance[5];
            //    combi[0].mesh = voetpadmesh;
            //    combi[1].mesh = fietspadMesh;
            //    combi[2].mesh = parkeervlakMesh;
            //    combi[3].mesh = wegmesh;
            //    combi[4].mesh = spoorbaanMesh;


            //    Mesh Roads = new Mesh();
            //    Roads.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    Roads.CombineMeshes(combi, true, false);
            //    Roads = SimplifyMesh(Roads, 0.05f);

            //    combi = new CombineInstance[3];
            //    combi[0].mesh = erfMesh;
            //    combi[1].mesh = LandUseMesh;
            //    combi[2].mesh = WoonerfMesh;

            //    Mesh landuse = new Mesh();
            //    landuse.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    landuse.CombineMeshes(combi, true, false);
            //    landuse = SimplifyMesh(landuse, 0.05f);

            //    combi = new CombineInstance[12];


            //    combi[0].mesh = CreateEmptyMesh(); //
            //    combi[1].mesh = CreateEmptyMesh(); //
            //    combi[2].mesh = CreateEmptyMesh(); //
            //    combi[3].mesh = Roads; //
            //    combi[4].mesh = SimplifyMesh(groenMesh, 0.05f);//
            //    combi[5].mesh = CreateEmptyMesh(); //
            //    combi[6].mesh = landuse; //
            //    combi[7].mesh = CreateEmptyMesh(); //
            //    combi[8].mesh = CreateEmptyMesh(); //
            //    combi[9].mesh = genericCityObjectMesh;
            //    combi[10].mesh = bridgeMesh;
            //    combi[11].mesh = waterBodyMesh;


            //    Mesh lod0Mesh = new Mesh();
            //    lod0Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //    lod0Mesh.CombineMeshes(combi, false, false);
            //    lod0Mesh.uv2 = RDuv2(lod0Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(originX, originY, 0)), tileSize);
            //    //Physics.BakeMesh(lod0Mesh.GetInstanceID(), false);
            //    AssetDatabase.CreateAsset(lod0Mesh, "Assets/GeneratedTileAssets/terrain_" + originX + "-" + originY + "-lod0.mesh");

            //}
        }
        public void CreateCombinedMeshes(Dictionary<terrainType, Mesh>meshes,Vector2 tileID, float tileSize)
        {
            //create LOD1 Mesh
            //UnityMeshSimplifier.MeshSimplifier meshSimplifier1 = null;
            //Thread thread1 = null;
            //UnityMeshSimplifier.MeshSimplifier meshSimplifier2 = null;
            //Thread thread2 = null;
            //UnityMeshSimplifier.MeshSimplifier meshSimplifier3 = null;
            //Thread thread3 = null;
            int vertcount = 0;
            CombineInstance[] combi = new CombineInstance[12];
            for (int i = 0; i < combi.Length; i++)
            {
                combi[i].mesh = CreateEmptyMesh();
                //combi[i].mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                vertcount += combi[i].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.voetpad))
            {
                combi[0].mesh = meshes[terrainType.voetpad]; //
                vertcount += combi[0].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.fietspad))
            {
                combi[1].mesh = meshes[terrainType.fietspad]; //
                vertcount += combi[1].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.parkeervakken))
            {
                combi[2].mesh = meshes[terrainType.parkeervakken]; //
                vertcount += combi[2].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.wegen))
            {
                combi[3].mesh = meshes[terrainType.wegen]; //
                vertcount += combi[3].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.begroeid))
            {
                combi[4].mesh = SimplifyMesh(meshes[terrainType.begroeid], 0.05f); //
                vertcount += combi[4].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.erven))
            {
                combi[5].mesh = SimplifyMesh(meshes[terrainType.erven], 0.05f); // //
                vertcount += combi[5].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.onbegroeid))
            {
                combi[6].mesh = SimplifyMesh(meshes[terrainType.onbegroeid], 0.05f); // //
                vertcount += combi[6].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.spoorbanen))
            {
                combi[7].mesh = meshes[terrainType.spoorbanen]; //
                vertcount += combi[7].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.woonerven))
            {
                combi[8].mesh = meshes[terrainType.woonerven]; //
                vertcount += combi[8].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.constructies))
            {
                combi[9].mesh = meshes[terrainType.constructies];
                vertcount += combi[9].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.bruggen))
            {
                combi[10].mesh = meshes[terrainType.bruggen];
                vertcount += combi[10].mesh.vertexCount;
            }
            if (meshes.ContainsKey(terrainType.water))
            {
                combi[11].mesh = meshes[terrainType.water];
                vertcount += combi[11].mesh.vertexCount;
            }

            Mesh lod1Mesh = new Mesh();
            if (vertcount > 65500)
            {
                lod1Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            lod1Mesh.CombineMeshes(combi, false, false);
            lod1Mesh.uv2 = RDuv2(lod1Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(tileID.x, tileID.y, 0)), tileSize);
            //Physics.BakeMesh(lod1Mesh.GetInstanceID(), false);
            //remove old asset

            //remove spikes
           lod1Mesh.vertices = RemoveSpikes(lod1Mesh).vertices;
            string baseMeshNameLod0 = "terrain_" + (int)tileID.x + "-" + (int)tileID.y + "-lod0";
            string assetName = "Assets/GeneratedTileAssets/terrain_" + (int)tileID.x + "-" + (int)tileID.y + "-lod1.mesh";
            string assetNameLod0 = "Assets/GeneratedTileAssets/terrain_" + (int)tileID.x + "-" + (int)tileID.y + "-lod0.mesh";
            Mesh existingMesh = (Mesh)AssetDatabase.LoadAssetAtPath(assetName, typeof(Mesh));


            vertcount = 0;
            for (int i = 0; i < combi.Length; i++)
            {
                combi[i].mesh = SimplifyMesh(combi[i].mesh, 0.05f);
                vertcount += combi[i].mesh.vertexCount;
            }

            Mesh lod0Mesh = new Mesh();
            if (vertcount > 65500)
            {
                lod0Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            lod0Mesh.CombineMeshes(combi, false, false);
            lod0Mesh.vertices = RemoveSpikes(lod0Mesh).vertices;
            lod0Mesh.uv2 = RDuv2(lod0Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(tileID.x, tileID.y, 0)), tileSize);

            
            if (existingMesh != null)
            {
                //combine meshes;
                lod1Mesh = CombineMeshes(lod1Mesh, (Mesh)AssetDatabase.LoadAssetAtPath(assetName, typeof(Mesh)));
                lod0Mesh = CombineMeshes(lod0Mesh, (Mesh)AssetDatabase.LoadAssetAtPath(assetNameLod0, typeof(Mesh)));
                AssetDatabase.DeleteAsset(assetName);
                AssetDatabase.DeleteAsset(assetNameLod0);
                AssetDatabase.SaveAssets();
            }
            lod1Mesh.Optimize();
            lod0Mesh.Optimize();
            AssetDatabase.CreateAsset(lod1Mesh, assetName);
            AssetDatabase.CreateAsset(lod0Mesh, assetNameLod0);
            AssetDatabase.SaveAssets();

            for (int i = 0; i < combi.Length; i++)
            {
                Destroy(combi[i].mesh);
            }

        }

        

        private Mesh RemoveSpikes(Mesh mesh)
        {
            if (mesh.vertices.Length == 0)
            {
                return mesh;
            };

            var verts = mesh.vertices;

            var correctverts = verts.Where(o => o.y < heightMax && o.y > heightMin);

            var correctvertsAvgHeight = verts.Average(o => o.y);

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

            return mesh;
        }

        private Mesh CombineMeshes(Mesh mesh1, Mesh mesh2)
        {
            Mesh newMesh = new Mesh();
            
            //newMesh.
            List<Vector3> verts = new List<Vector3>(mesh1.vertices);
            verts.AddRange(mesh2.vertices);
            List<Vector2> newUVs = new List<Vector2>(mesh1.uv2);
            newUVs.AddRange(new List<Vector2>(mesh2.uv2));

            if (verts.Count> 65500)
            {
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            newMesh.vertices = verts.ToArray();
            newMesh.uv2 = newUVs.ToArray();
            newMesh.subMeshCount = mesh1.subMeshCount;
            int meshVertexCount = mesh1.vertexCount;
            for (int submeshIndex = 0; submeshIndex < mesh1.subMeshCount; submeshIndex++)
            {
                // = new List<int>();
                List<int> submeshIndices = new List<int>(mesh1.GetIndices(submeshIndex));
                int subMesh1BaseVertex = mesh1.GetSubMesh(submeshIndex).baseVertex;
                for (int i = 0; i < submeshIndices.Count; i++)
                {
                    submeshIndices[i] += subMesh1BaseVertex;
                }
                List<int> extraIndices = new List<int>(mesh2.GetIndices(submeshIndex));

                int mesh2BaseVertex = mesh2.GetSubMesh(submeshIndex).baseVertex;
                for (int i = 0; i < extraIndices.Count; i++)
                {
                    extraIndices[i] += meshVertexCount+mesh2BaseVertex;
                }
                submeshIndices.AddRange(extraIndices);
                newMesh.SetIndices(submeshIndices, MeshTopology.Triangles, submeshIndex);
            }
           // DestroyImmediate(mesh1,true);
            
            //DestroyImmediate(mesh2,true);
            newMesh.RecalculateNormals();
            
            return newMesh;
            }
        private Mesh SimplifyMesh(Mesh mesh, float quality)
        {

            if (mesh.triangles.Length < 100)
            {
                return copyMesh(mesh);
            }

            var DecimatedMesh = mesh;

            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.Initialize(DecimatedMesh);
            meshSimplifier.PreserveBorderEdges = true;
            meshSimplifier.MaxIterationCount = 500;
            meshSimplifier.SimplifyMesh(quality);
            meshSimplifier.EnableSmartLink = true;
            DecimatedMesh = meshSimplifier.ToMesh();
            DecimatedMesh.RecalculateNormals();
            DecimatedMesh.Optimize();
            return DecimatedMesh;
        }

        private Mesh copyMesh(Mesh mesh)
        {
            Mesh newMesh = new Mesh();
            newMesh.name = mesh.name;
            newMesh.indexFormat = mesh.indexFormat;
            newMesh.vertices = mesh.vertices;
            int submeshcount = mesh.subMeshCount;
            newMesh.subMeshCount = submeshcount;
            for (int i = 0; i < submeshcount; i++)
            {
                newMesh.SetTriangles(mesh.GetTriangles(i), i);
            }
            newMesh.normals = mesh.normals;
            return newMesh;

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
            List<Vector3> vectors = new List<Vector3>();

            List<Vector3> clipboundary = CreateClippingPolygon(tileSize);

            if (RDTriangles.Count == 0)
            {
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


                List<Vector3> defshape = Netherlands3D.Utilities.TriangleClipping.SutherlandHodgman.ClipPolygon(vectors, clipboundary);

                if (defshape.Count < 3)
                {
                    continue;
                }

                if (defshape[0].x.ToString() == "NaN")
                {
                    continue;
                }

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
            }

            //createMesh
            List<Vector3> verts = new List<Vector3>();
            Vector3RD tileCenterRD = new Vector3RD();
            tileCenterRD.x = originX + (tileSize / 2);
            tileCenterRD.y = originY + (tileSize / 2);
            tileCenterRD.z = 0;
            Vector3 tileCenterUnity = CoordConvert.RDtoUnity(tileCenterRD);
            List<int> ints = new List<int>();
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
                return CreateEmptyMesh();
            }
            Mesh mesh = new Mesh();
            if (verts.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.vertices = verts.ToArray();
            mesh.triangles = ints.ToArray();
            mesh = WeldVertices(mesh);
            mesh.RecalculateNormals();
            mesh.Optimize();
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

        private Mesh WeldVertices(Mesh mesh)
        {
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

            return mesh;
        }


        void Import()
        {
            int Xmin = (int)Config.activeConfiguration.BottomLeftRD.x;
            int Ymin = (int)Config.activeConfiguration.BottomLeftRD.y;
            int Xmax = (int)Config.activeConfiguration.TopRightRD.x;
            int Ymax = (int)Config.activeConfiguration.TopRightRD.y;

            int stepSize = 1000;

            //debug
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //Xmax = 110000;
            //Ymax = 479000;
            //sw.Start();
            for (int X = Xmin; X < Xmax; X += stepSize)
            {
                for (int Y = Ymin; Y < Ymax; Y += stepSize)
                {
                    Debug.Log(X + "=" + Y);
                    ImportSingle(X, Y);
                }
            }
            //sw.Stop();
            //Debug.Log(sw.ElapsedMilliseconds + " ms");
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