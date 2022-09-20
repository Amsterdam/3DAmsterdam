using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using System.Linq;
using System;


namespace T3D.LoadData
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

    public class CityJsonModel
    {
        public JSONNode cityjsonNode;
        public List<Vector3Double> vertices;
        private List<Vector2> textureVertices;
        private List<Surfacetexture> Textures;
        private List<material> Materials;

        private double LOD;

        private Vector3Double transformScale;
        private Vector3Double transformOffset;

        public Vector3Double TransformOffset { get => transformOffset; }
        /// <summary>
        /// The constructor loads the vertices and applies the scale and translate parameters and converts from RD/WGS84 to Unity
        /// </summary>
        /// <param name="jsonstring">The cityjson string</param>
        public CityJsonModel(string jsonstring, Vector3RD centerWorld, bool checkDistanceFromCenter)
        {

            cityjsonNode = JSON.Parse(jsonstring);

            //get vertices
            vertices = new List<Vector3Double>();
            textureVertices = new List<Vector2>();
            Textures = new List<Surfacetexture>();

            transformScale = (cityjsonNode["transform"] != null && cityjsonNode["transform"]["scale"] != null) ? new Vector3Double(
                cityjsonNode["transform"]["scale"][0].AsDouble,
                cityjsonNode["transform"]["scale"][1].AsDouble,
                cityjsonNode["transform"]["scale"][2].AsDouble
            ) : new Vector3Double(1, 1, 1);

            transformOffset = (cityjsonNode["transform"] != null && cityjsonNode["transform"]["translate"] != null) ? new Vector3Double(
                   cityjsonNode["transform"]["translate"][0].AsDouble,
                   cityjsonNode["transform"]["translate"][1].AsDouble,
                   cityjsonNode["transform"]["translate"][2].AsDouble
            ) : new Vector3Double(0, 0, 0);


            var vertarray = cityjsonNode["vertices"].Linq;

            if (vertarray.Count() == 0)
                ErrorService.GoToErrorPage("Geometry not found error: Vertex list is empty.\nThe downloaded CityJSON contains no vertices.");

            double minx, miny, minz, maxx, maxy, maxz;

            //var extents = cityjsonNode["metadata"]["geographicalExtent"];
            //if (extents != null)
            //{
            //    minx = extents.AsArray[0].AsDouble;
            //    miny = extents.AsArray[1].AsDouble;
            //    minz = extents.AsArray[2].AsDouble;
            //    maxx = extents.AsArray[3].AsDouble;
            //    maxy = extents.AsArray[4].AsDouble;
            //    maxz = extents.AsArray[5].AsDouble;
            //}
            //else
            //{
            minx = vertarray.Min(o => o.Value[0].AsDouble * transformScale.x);
            miny = vertarray.Min(o => o.Value[1].AsDouble * transformScale.y);
            minz = vertarray.Min(o => o.Value[2].AsDouble * transformScale.z);

            maxx = vertarray.Max(o => o.Value[0].AsDouble * transformScale.x);
            maxy = vertarray.Max(o => o.Value[1].AsDouble * transformScale.y);
            maxz = vertarray.Max(o => o.Value[2].AsDouble * transformScale.z);
            //}

            if ((maxx - minx > 500) || (maxy - miny > 500))
            {
                ErrorService.GoToErrorPage("Cannot parse CityJSON: Distance between vertices too large.. ");
            }
            var centerx = minx + ((maxx - minx) / 2);
            var centery = miny + ((maxy - miny) / 2);

            //now load all the vertices with the scaler and offset applied
            foreach (JSONNode node in cityjsonNode["vertices"])
            {
                var rd = new Vector3RD(
                        node[0].AsDouble * transformScale.x + transformOffset.x,
                        node[1].AsDouble * transformScale.y + transformOffset.y,
                        node[2].AsDouble * transformScale.z + transformOffset.z
                );

                if (IsValidRD(rd))
                {
                    if (checkDistanceFromCenter)
                    {
                        var center = Netherlands3D.Config.activeConfiguration.RelativeCenterRD;
                        var check_x = Math.Abs(rd.x - center.x);
                        var check_y = Math.Abs(rd.y - center.y);

                        var perceelRadius = RestrictionChecker.ActivePerceel.Radius;

                        if (check_x > perceelRadius || check_y > perceelRadius)
                        {
                            var vertCoordinates = new Vector3Double(rd.x - centerx, rd.z + centerWorld.z, rd.y - centery);
                            vertices.Add(vertCoordinates);
                        }
                        else
                        {
                            AddToVertices(rd);
                        }
                    }
                    else
                    {
                        AddToVertices(rd);
                    }
                }
                else //is it WGS84 or is it Unity coordinate
                {
                    Vector3WGS wgs = new Vector3WGS(rd.x, rd.y, rd.z);

                    if (IsValidWGS84(wgs))
                    {
                        var unityCoordinates = CoordConvert.WGS84toUnity(wgs);
                        var vertCoordinates = new Vector3Double(unityCoordinates.x, unityCoordinates.z, unityCoordinates.y);
                        vertices.Add(vertCoordinates);
                    }
                    else
                    {
                        //var posRd = new Vector3RD(centerWorld.x + rd.x - centerx, centerWorld.y + rd.y - centery, centerWorld.z + rd.z);
                        var vertCoordinates = new Vector3Double(rd.x - centerx, rd.z + centerWorld.z, rd.y - centery);
                        vertices.Add(vertCoordinates);
                    }
                }
            }
            //get textureVertices
            foreach (JSONNode node in cityjsonNode["appearance"]["vertices-texture"])
            {
                textureVertices.Add(new Vector2(node[0].AsFloat, node[1].AsFloat));
            }
            foreach (JSONNode node in cityjsonNode["appearance"]["textures"])
            {
                Surfacetexture texture = new Surfacetexture();
                texture.path = node["image"];
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

        private void AddToVertices(Vector3RD rd)
        {
            var unityCoordinates = CoordConvert.RDtoUnity(rd);
            var vertCoordinates = new Vector3Double(unityCoordinates.x, unityCoordinates.z, unityCoordinates.y);
            vertices.Add(vertCoordinates);
        }

        public List<Building> LoadBuildings(double lod)
        {
            List<Building> buildings = new List<Building>();
            LOD = lod;
            foreach (JSONNode node in cityjsonNode["CityObjects"])
            {

                Building bldg = ReadBuilding(node);
                if (bldg != null)

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
                if (geometrynode["lod"].AsDouble == LOD)
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
                if (geometrynode["lod"].AsDouble == LOD)
                {
                    if (geometrynode["type"] == "Solid")
                    {

                        JSONNode exteriorshell = geometrynode["boundaries"][0];
                        foreach (JSONNode surfacenode in exteriorshell)
                        {
                            surfaces.Add(ReadSurfaceVectors(surfacenode));
                        }
                        JSONNode interiorshell = geometrynode[0][1];
                        if (interiorshell != null)
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
                    if (texturenode != null)
                    {
                        int counter = 0;
                        JSONNode valuesNode = texturenode[0]["values"];
                        if (geometrynode["type"] == "Solid")
                        {
                            JSONNode exteriorshell = valuesNode[0];
                            foreach (JSONNode surfacenode in exteriorshell)
                            {
                                surfaces[counter] = AddSurfaceUVs(surfacenode, surfaces[counter]);
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
                    if (semanticsnode != null)
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
                    if (child != null)
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
                {
                    if (counter > 0)
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

            foreach (KeyValuePair<string, JSONNode> kvp in semanticsNode)
            {
                result.Add(new Semantics(kvp.Key, kvp.Value));
            }

            return result;
        }

        public static bool IsValidRD(Vector3RD coordinaat)
        {
            if (coordinaat.x > 7000 && coordinaat.x < 300000)
            {
                if (coordinaat.y > 289000 && coordinaat.y < 629000)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidWGS84(Vector3WGS coordinaat)
        {
            if (coordinaat.lon > 3.29804 && coordinaat.lon < 7.57893)
            {
                if (coordinaat.lat > 50.57222 && coordinaat.lat < 53.62702)
                {
                    return true;
                }
            }
            return false;
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
            semantics = new List<Semantics>();
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
