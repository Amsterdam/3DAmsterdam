using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System;
using System.Globalization;

namespace cityJSON
{
    [System.Serializable]
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;
        public Vector3Double(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }


    public class CityModel
    {
        public JSONNode cityjsonNode;
        public List<Vector3Double> vertices;
        private List<Vector2>textureVertices;
        private List<Surfacetexture> Textures;
        private List<material> Materials;

        private int LOD;
        public CityModel(string filepath, string filename)
        {
            string jsonstring;
            jsonstring = File.ReadAllText(filepath+filename);
            cityjsonNode = JSON.Parse(jsonstring);
            
            //get vertices
            vertices = new List<Vector3Double>();
            textureVertices = new List<Vector2>();
            Textures = new List<Surfacetexture>();
            foreach (JSONNode node in cityjsonNode["vertices"])
            {

                vertices.Add(new Vector3Double(double.Parse(node[0].Value,CultureInfo.InvariantCulture), double.Parse(node[1].Value, CultureInfo.InvariantCulture), double.Parse(node[2].Value, CultureInfo.InvariantCulture)));
            }
            //get textureVertices
            foreach (JSONNode node in cityjsonNode["appearance"]["vertices-texture"])
            {
                textureVertices.Add(new Vector2(node[0].AsFloat, node[1].AsFloat));
            }
            foreach (JSONNode node in cityjsonNode["appearance"]["textures"])
            {
                Surfacetexture texture = new Surfacetexture();
                texture.path = filepath + node["image"];
                texture.wrapmode = node["wrapMode"];
                Textures.Add(texture);
            }
            
            //get materials
            //foreach (JSONNode node in cityjsonNode["appearance"]["materials"])
            //{
            //    material mat = new material();
            //    mat.name = node["name"];
            //    JSONNode diffColor;
            //    diffColor = node["diffuseColor"];
            //    if (diffColor!=null)
            //    {
            //        mat.r = diffColor[0];
            //        mat.g = diffColor[1];
            //        mat.b = diffColor[2];
            //    }
            //    Materials.Add(mat);
            //}
        }

        public List<Building> LoadBuildings(int lod)
        {
            List<Building> buildings = new List<Building>();
            LOD = lod;
            foreach (JSONNode node in cityjsonNode["CityObjects"])
            {

                        Building bldg = ReadBuilding(node);
                        if (bldg !=null)

                        {
                            buildings.Add(bldg);
                        }

            }
            return buildings;
        }

        private Building ReadBuilding(JSONNode node)
        {
            Building bldg = new Building();
            bool LODcorrect = false;
            foreach (JSONNode geometrynode in node["geometry"])
            {
                if (geometrynode["lod"].AsInt == LOD)
                {
                    LODcorrect = true;
                }
            }
            if (LODcorrect == false)
            {
                return null;
            }
            if (node["parents"] != null)
            {
                return null;
            }
            //read attributes
            List<Semantics> semantics = ReadSemantics(node["attributes"]);

            bldg.semantics = semantics;

            //readSurfaceGeometry
            List<Surface> surfaces = new List<Surface>();
            foreach (JSONNode geometrynode in node["geometry"])
            {
                if (geometrynode["lod"].AsInt == LOD)
                {
                    if (geometrynode["type"] == "Solid")
                    {

                        JSONNode exteriorshell = geometrynode["boundaries"][0];
                        foreach (JSONNode surfacenode in exteriorshell)
                        {
                            surfaces.Add(ReadSurfaceVectors(surfacenode));
                        }
                        JSONNode interiorshell = geometrynode[0][1];
                        if (interiorshell !=null)
                        {
                            foreach (JSONNode surfacenode in interiorshell)
                            {
                                surfaces.Add(ReadSurfaceVectors(surfacenode));
                            }
                        }

                    }
                    if (geometrynode["type"] == "MultiSurface")
                    {
                        foreach (JSONNode surfacenode in geometrynode["boundaries"])
                        {
                            surfaces.Add(ReadSurfaceVectors(surfacenode));
                        }
                        
                    }

                    //read textureValues
                    JSONNode texturenode = geometrynode["texture"];
                    if (texturenode!=null)
                    {
                        int counter = 0;
                        JSONNode valuesNode = texturenode[0]["values"];
                        if (geometrynode["type"] == "Solid")
                        {
                            JSONNode exteriorshell = valuesNode[0];
                            foreach (JSONNode surfacenode in exteriorshell)
                            {
                                surfaces[counter] = AddSurfaceUVs(surfacenode,surfaces[counter]);
                                counter++;
                            }
                            JSONNode interiorshell = valuesNode[1];
                            if (interiorshell != null)
                            {
                                foreach (JSONNode surfacenode in interiorshell)
                                {
                                    surfaces[counter] = AddSurfaceUVs(surfacenode, surfaces[counter]);
                                    counter++;
                                }
                            }
                        }
                        if (geometrynode["type"] == "MultiSurface")
                        {
                            foreach (JSONNode surfacenode in valuesNode)
                            {
                                surfaces[counter] = AddSurfaceUVs(surfacenode, surfaces[counter]);
                                counter++;
                            }
                        }
                    }
                    
                    
                    //read SurfaceAttributes
                    JSONNode semanticsnode = geometrynode["semantics"];
                    if (semanticsnode!=null)
                    {
                        if (geometrynode["type"] == "Solid")
                        {
                            for (int i = 0; i < semanticsnode["values"][0].Count; i++)
                            {
                                int semanticsID = semanticsnode["values"][0][i].AsInt;
                                surfaces[i].semantics = ReadSemantics(semanticsnode["surfaces"][semanticsID]);
                            }
                        }
                        if (geometrynode["type"] == "MultiSurface")
                        {
                            for (int i = 0; i < semanticsnode["values"].Count; i++)
                            {
                                int semanticsID = semanticsnode["values"][i].AsInt;
                                surfaces[i].semantics = ReadSemantics(semanticsnode["surfaces"][semanticsID]);
                            }
                        }

                    }

                    // read MaterialValues
                    JSONNode materialnode = geometrynode["material"];

                }
            }

            bldg.surfaces = surfaces;

            
            //process children
            
            JSONNode childrenNode = node["children"];
            if (childrenNode != null)
            {
                List<Building> children = new List<Building>();
                for (int i = 0; i < childrenNode.Count; i++)
                {
                    string childname = childrenNode[i];
                    JSONNode childnode = cityjsonNode["CityObjects"][childname];

                    Building child = ReadBuilding(childnode);
                    if (child!=null)
                    {
                        children.Add(child);
                    }
                }
                bldg.children = children;
            }
            return bldg;
        }

        private List<Surface> ReadSolid(JSONNode geometrynode)
        {
            JSONNode boundariesNode = geometrynode["boundaries"];
            List<Surface> result = new List<Surface>();

            foreach (JSONNode node in boundariesNode[0])
            {
                Surface surf = new Surface();
                foreach (JSONNode vertexnode in node[0])
                {
                    surf.outerRing.Add(vertices[vertexnode.AsInt]);
                }
                result.Add(surf);
            }
            JSONNode semanticsnode = geometrynode["semantics"];
            JSONNode ValuesNode = semanticsnode["values"][0];
            for (int i = 0; i < ValuesNode.Count; i++)
            {
                result[i].SurfaceType = geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]["type"];
                result[i].semantics = ReadSemantics(geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]);
            }

            if (geometrynode["texture"] != null)
            {
                Surfacetexture surfacematerial = null;
                int counter = 0;
                foreach (JSONNode node in geometrynode["texture"][0][0])
                {
                    List<Vector2> indices = new List<Vector2>();
                    for (int i = 0; i < node[0][0].Count; i++)
                    {
                        JSONNode item = node[0][0][i];
                        
                        if (surfacematerial is null)
                        {
                            surfacematerial = Textures[item.AsInt];
                            result[i].surfacetexture = surfacematerial;
                        }
                        else
                        {
                            indices.Add(textureVertices[item.AsInt]);
                        }

                    }
                    indices.Reverse();
                    result[counter].outerringUVs = indices;
                    counter++;
                }
            }
            return result;
        }

        private List<Surface> ReadMultiSurface(JSONNode geometrynode)
        {
            JSONNode boundariesNode = geometrynode["boundaries"];
            List<Surface> result = new List<Surface>();
            foreach (JSONNode node in boundariesNode)
            {
                Surface surf = new Surface();
                foreach (JSONNode vertexnode in node[0])
                {
                    surf.outerRing.Add(vertices[vertexnode.AsInt]);

                }
                for (int i = 1; i < node.Count; i++)
                {
                    List<Vector3Double> innerRing = new List<Vector3Double>();
                    foreach (JSONNode vertexnode in node[i])
                    {
                        innerRing.Add(vertices[vertexnode.AsInt]);

                    }
                    surf.innerRings.Add(innerRing);
                }
                result.Add(surf);
            }
            //add semantics
            JSONNode semanticsnode = geometrynode["semantics"];
            JSONNode ValuesNode = semanticsnode["values"];
            for (int i = 0; i < ValuesNode.Count; i++)
            {
                string surfacetype = geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]["type"];
                result[i].SurfaceType = geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]["type"];
                result[i].semantics = ReadSemantics(geometrynode["semantics"]["surfaces"][ValuesNode[i].AsInt]);
            }
            //if (geometrynode["texture"] != null)
            //{
            //    Surfacetexture surfacematerial = null;
            //    int counter = 0;
            //    foreach (JSONNode node in geometrynode["texture"][0][0])
            //    {
            //        List<Vector2> indices = new List<Vector2>();
            //        for (int i = 0; i < node[0].Count; i++)
            //        {
            //            JSONNode item = node[0][i];

            //            if (surfacematerial is null)
            //            {
            //                surfacematerial = Textures[item.AsInt];
            //                result[i].surfacetexture = surfacematerial;
            //            }
            //            else
            //            {
            //                indices.Add(textureVertices[item.AsInt]);
            //            }

            //        }
            //        indices.Reverse();
            //        result[counter].outerringUVs = indices;
            //        counter++;
            //    }
            //}

            return result;
        }


        private Surface AddSurfaceUVs(JSONNode UVValueNode, Surface surf)
        {
            List<Vector2> UVs = new List<Vector2>();
            
            foreach (JSONNode vectornode in UVValueNode[0])
            {
                if (surf.TextureNumber == -1)
                {
                    surf.TextureNumber = vectornode.AsInt;
                    surf.surfacetexture = Textures[surf.TextureNumber];
                }
                else
                {
                    UVs.Add(textureVertices[vectornode.AsInt]);
                }
            }
            UVs.Reverse();
            surf.outerringUVs = UVs;

            //innerrings

            for (int i = 1; i < UVValueNode.Count; i++)
            {
                UVs = new List<Vector2>();
                int counter = 0;
                foreach (JSONNode vectornode in UVValueNode[i])
                {   if (counter > 0)
                    {
                        UVs.Add(textureVertices[vectornode.AsInt]);
                    }
                    counter++;
                }
                UVs.Reverse();
                surf.innerringUVs.Add(UVs);
            }

            return surf;
        }
       private Surface ReadSurfaceVectors(JSONNode surfacenode)
        {
            Surface surf = new Surface();
            //read exteriorRing
            List<Vector3Double> verts = new List<Vector3Double>();
            foreach (JSONNode vectornode in surfacenode[0])
            {
                verts.Add(vertices[vectornode.AsInt]);
            }
            surf.outerRing = verts;
            for (int i = 1; i < surfacenode.Count; i++)
            {
                verts = new List<Vector3Double>();
                foreach (JSONNode vectornode in surfacenode[i])
                {
                    verts.Add(vertices[vectornode.AsInt]);
                }
                surf.innerRings.Add(verts);
            }

            return surf;
        }

        private List<Semantics> ReadSemantics(JSONNode semanticsNode)
        {
            List<Semantics> result = new List<Semantics>();

            foreach (KeyValuePair<string, JSONNode> kvp in (JSONObject)semanticsNode)
            {
                result.Add(new Semantics(kvp.Key, kvp.Value));
            }

            return result;
        }

        
    }

    public class Building
    {
        public List<Semantics> semantics;
        public List<Surface> surfaces;
        public List<Building> children;
            public Building()
        {
            semantics = new List<Semantics>();
            surfaces = new List<Surface>();
            children = new List<Building>();
        }
    }

    public class Surface
    {
        public List<Vector3Double> outerRing;
        public List<List<Vector3Double>> innerRings;
        public List<Vector2> outerringUVs;
        public List<List<Vector2>> innerringUVs;
        public string SurfaceType;
        public List<Semantics> semantics;
        public Surfacetexture surfacetexture;
        public int TextureNumber;

        public Surface()
        {
            TextureNumber = -1;
            semantics  = new List<Semantics>();
            outerRing = new List<Vector3Double>();
            innerRings = new List<List<Vector3Double>>();
            outerringUVs = new List<Vector2>();
            innerringUVs = new List<List<Vector2>>();
        }
    }

    public class Surfacetexture
    {
        public string path;
        public string wrapmode;
    }

    [System.Serializable]
    public class Semantics
    {
        public string name;
        public string value;
        public Semantics(string Name, string Value)
        {
            name = Name;
            value = Value;
        }
    }

    public class material
    {
        public string name;
        public float r;
        public float g;
        public float b;
        public float a;
    }
}
