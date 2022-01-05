#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;

using TextureCombiner;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public class CreateGameObjectsV2
    {     
        private Vector3 offset;
        public GameObject CreateMeshesByIdentifier(List<Building> buildings, string identifiername, Vector3RD origin)
        {
            offset = CoordConvert.RDtoUnity(new Vector2((float)origin.x, (float)origin.y));
            GameObject container = new GameObject();
            ObjectMapping objMap = container.AddComponent<ObjectMapping>();
            List<string> identifiers = new List<string>();
            List<List<SurfaceData>> surfdata = new List<List<SurfaceData>>();
            int buildingnumber = 0;
            foreach (Building building in buildings)
            {
                string buildingname = "";
                foreach (Semantics item in building.semantics)
                {
                    if( item.name == "name")
                    {
                        buildingname = item.value;
                    }
                }
                objMap.Objectmap.Add(buildingnumber, buildingname);
                objMap.BagID.Add(buildingname);
                buildingnumber++;
                List<SurfaceData> surfaces = SortSurfacesBySemantics(building, identifiername,buildingnumber);

                foreach (SurfaceData surf in surfaces)
                {
                    bool found = false;
                    for (int i = 0; i < identifiers.Count; i++)
                    {
                        if (identifiers[i] == surf.identifierValue)
                        {
                            found = true;
                            surfdata[i].Add(surf);
                        }
                    }
                    if (found == false)
                    {
                        identifiers.Add(surf.identifierValue);
                        surfdata.Add(new List<SurfaceData>());
                        surfdata[surfdata.Count - 1].Add(surf);
                    }
                }
            }
            
            int vertexcount = 0;
            int counter = 0;
            for (int i = 0; i < identifiers.Count; i++)
            {
                List<CombineInstance> cis = new List<CombineInstance>();
                foreach (SurfaceData surf in surfdata[i])
                {
                    if (vertexcount + surf.texturedMesh.mesh.vertexCount>65535)
                    {
                        AddSubobject(container, cis, identifiers[i],counter);
                        counter += 1;
                        cis = new List<CombineInstance>();
                        vertexcount = 0;
                    }
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = surf.texturedMesh.mesh;
                    cis.Add(ci);
                    surf.texturedMesh.mesh = null;
                   
                    vertexcount += ci.mesh.vertexCount;
                }
                surfdata[i] = null;
                AddSubobject(container, cis, identifiers[i],counter);

            }

            return container;
        }
        private void AddSubobject(GameObject container,List<CombineInstance> cis, string identifier, int counter)
        {
            GameObject go = new GameObject();
            go.transform.parent = container.transform;
            go.name = identifier + "_" + counter; ;
            Mesh combimesh = new Mesh();
            combimesh.CombineMeshes(cis.ToArray(), true, false);
            go.AddComponent<MeshFilter>().sharedMesh = combimesh;
            Material mat = Resources.Load("CityJSON/" + identifier, typeof(Material)) as Material;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }
        public List<SurfaceData> SortSurfacesBySemantics(Building building, string identifiername,int buildingnr, string Buildingname ="")
        {
            string buildingname = Buildingname;
            string buildingpartname = "";
            for (int i = 0; i < building.semantics.Count; i++)
            {
                if (buildingname == "")
                {
                    if (building.semantics[i].name == "name")
                    {
                        buildingname = building.semantics[i].value;
                        i = building.semantics.Count;
                    }
                }
                else
                {
                    if (building.semantics[i].name == "type")
                    {
                        buildingpartname = building.semantics[i].value;
                        i = building.semantics.Count;
                    }
                }
            }

            List<SurfaceData> output = new List<SurfaceData>();
            foreach (Surface surf in building.surfaces)
            {
                SurfaceData surfdata = new SurfaceData();
                surfdata.Buildingname = buildingname;
                surfdata.buildingpartname = buildingpartname;
                string identifiervalue = "";
                for (int i = 0; i < surf.semantics.Count; i++)
                {
                    if (surf.semantics[i].name == identifiername)
                    {
                        identifiervalue = surf.semantics[i].value;
                        surfdata.identifierValue = identifiervalue;
                        i = surf.semantics.Count;
                    }
                    surfdata.texturedMesh = CreateSurface(surf);
                    Vector2 uv = new Vector2(buildingnr, buildingnr);
                    Vector2[] uv4 = new Vector2[surfdata.texturedMesh.mesh.vertexCount];
                    for (int j = 0; j < uv4.Length; j++)
                    {
                        uv4[j] = uv;
                    }
                    surfdata.texturedMesh.mesh.uv4 = uv4;

                }
                output.Add(surfdata);
            }
            foreach (Building child in building.children)
            {
                List<SurfaceData> subtextures = SortSurfacesBySemantics(child,identifiername,buildingnr,buildingname);
                foreach (SurfaceData item in subtextures)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        private TexturedMesh CreateSurface(Surface surf)
        {
            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
            poly.outside = CreateVectorlist(surf.outerRing);
            if (surf.outerringUVs.Count > 0)
            {
                surf.outerringUVs.Reverse();
                poly.outsideUVs = surf.outerringUVs;
            };
            if (surf.innerRings.Count > 0)
            {
                foreach (var innerring in surf.innerRings)
                {
                    poly.holes.Add(CreateVectorlist(innerring));
                }
            }
            if (surf.innerringUVs.Count > 0)
            {
                surf.innerringUVs.Reverse();
                poly.holesUVs = surf.innerringUVs;
            }

            Mesh submesh = Poly2Mesh.CreateMesh(poly);

                TexturedMesh texturedmesh = new TexturedMesh();
                texturedmesh.mesh = submesh;
                if (surf.surfacetexture != null)
                {
                    texturedmesh.TextureName = surf.surfacetexture.path;
                    //texturedmesh.texture = LoadTexture(surf.surfacetexture);
                }
            submesh = null;
                return texturedmesh;
            
            
        }
        private List<Vector3> CreateVectorlist(List<Vector3Double> vectors)
        {
            List<Vector3> output = new List<Vector3>();
            Vector3 vect;
            for (int i = 0; i < vectors.Count; i++)
            {
                vect = CoordConvert.RDtoUnity(new Vector3RD(vectors[i].x,vectors[i].y,vectors[i].z))-offset;
                output.Add(vect);
            }
            output.Reverse();
            return output;
        }
    }

    public class SurfaceData
    {
        public TexturedMesh texturedMesh;
        public string Buildingname;
        public string buildingpartname;
        public string identifierValue;
    }


}
#endif