using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;

using TextureCombiner;
namespace cityJSON
{

    public class CreateGameObjectsV2
    {

        public void CreateMeshesByIdentifier(List<Building> buildings, string identifiername)
        {
            List<string> identifiers = new List<string>();
            List<List<SurfaceData>> surfdata = new List<List<SurfaceData>>();
            foreach (Building building in buildings)
            {
                List<SurfaceData> surfaces = SortSurfacesBySemantics(building, identifiername);
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

            for (int i = 0; i < identifiers.Count; i++)
            {
                List<CombineInstance> cis = new List<CombineInstance>();
                foreach (SurfaceData surf in surfdata[i])
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = surf.texturedMesh.mesh;
                    cis.Add(ci);
                }
                GameObject go = new GameObject();
                go.name = identifiers[i];
                Mesh combimesh = new Mesh();
                combimesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combimesh.CombineMeshes(cis.ToArray(), true, false);
                go.AddComponent<MeshFilter>().sharedMesh = combimesh;
                Material mat = Resources.Load("CityJSON/"+ identifiers[i], typeof(Material)) as Material;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            }


        }





        public List<SurfaceData> SortSurfacesBySemantics(Building building, string identifiername, string Buildingname ="")
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
                }
                output.Add(surfdata);
            }
            foreach (Building child in building.children)
            {
                List<SurfaceData> subtextures = SortSurfacesBySemantics(child,identifiername,buildingname);
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
                vect = CoordConvert.RDtoUnity(new Vector3RD(vectors[i].x,vectors[i].y,vectors[i].z));
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
