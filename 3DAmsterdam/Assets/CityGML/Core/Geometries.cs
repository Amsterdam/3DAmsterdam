using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

namespace CityGML
{
    [System.Serializable]
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;

        public Vector3Double(double X = 0, double Y = 0, double Z = 0)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }

    public class Geometries : MonoBehaviour
    {

    }

    public class MultiSurfaceProperty
    {
        XmlNode multisurfacenode;
        string ID;
        string type;
        XmlNode geometrynode;
        XmlNamespaceManager nsmr;

        public MultiSurfaceProperty(XmlNode node, XmlNamespaceManager namespaceManager)
        {
            multisurfacenode = node;
            nsmr = namespaceManager;
        }



    }


    public class LinearRing
    {
        public List<Vector3Double> Vertices;
        public List<Vector2> uvs;
        public string id;
        public LinearRing(List<Vector3Double> verts, string ID, List<Vector2> uvlist)
        {
            Vertices = verts;
            id = ID;
            uvs = uvlist;
        }
    }

    public class Surface
    {
        public string id;
        public string SurfaceTheme;
        public List<AttributeData> Attributes = new List<AttributeData>();
        public List<LinearRing> InteriorRings = new List<LinearRing>();
        public List<LinearRing> ExteriorRings = new List<LinearRing>();
        public ParameterizedTexture parameterizedTexture;

        public Surface(XmlNode SurfaceMemberNode, XmlNamespaceManager nsmr, AppearanceMember appearanceMember)
        {

            id = SurfaceMemberNode.FirstChild.Attributes["gml:id"].Value;

            if (appearanceMember.parametrizedTextures.ContainsKey(id))
            {
                parameterizedTexture = appearanceMember.parametrizedTextures[id];
            }

            string polygonid;
            XmlNode polygonnode = SurfaceMemberNode.SelectSingleNode("gml:Polygon", nsmr);
            if (polygonnode == null)
            {
                polygonnode = SurfaceMemberNode.SelectSingleNode("gml:CompositeSurface/gml:surfaceMember/gml:Polygon", nsmr);
            }

            polygonid = polygonnode.Attributes["gml:id"].Value; ;

            //"gml: CompositeSurface / gml:surfaceMember / gml:Polygon""

            XmlNodeList ExteriorRingnodes = SurfaceMemberNode.SelectNodes("gml:Polygon/gml:exterior/gml:LinearRing", nsmr);
            foreach (XmlNode ringnode in ExteriorRingnodes)
            {
                List<Vector3Double> poscoords = new List<Vector3Double>();
                XmlNode poslistnode = ringnode.SelectSingleNode("gml:posList", nsmr);
                if (poslistnode != null)
                {
                    List<Vector2> uv = null;
                    if (parameterizedTexture!=null)
                    {
                        uv = parameterizedTexture.targets[ringnode.Attributes["gml:id"].Value];
                    }
                    XmlAttribute idattribute = ringnode.Attributes["gml:id"];
                    if (idattribute != null) { string id = ringnode.Attributes["gml:id"].Value; }
                    ExteriorRings.Add(new LinearRing(ReadLinearRing(poslistnode), id,uv));
                }
            }

            XmlNodeList InteriorRingnodes = SurfaceMemberNode.SelectNodes("gml:Polygon/gml:interior/gml:LinearRing", nsmr);
            foreach (XmlNode ringnode in InteriorRingnodes)
            {
                
                List<Vector3Double> poscoords = new List<Vector3Double>();
                XmlNode poslistnode = ringnode.SelectSingleNode("gml:posList", nsmr);
                if (poslistnode != null)
                {
                    List<Vector2> uv = null;
                    if (parameterizedTexture != null)
                    {
                        uv = parameterizedTexture.targets[ringnode.Attributes["gml:id"].Value];
                    }
                    string id = ringnode.Attributes["gml:id"].Value;
                    InteriorRings.Add(new LinearRing(ReadLinearRing(poslistnode), id, uv)); ;
                }
            }

            ExteriorRingnodes = SurfaceMemberNode.SelectNodes("gml:CompositeSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:LinearRing", nsmr);
            foreach (XmlNode ringnode in ExteriorRingnodes)
            {
                List<Vector3Double> poscoords = new List<Vector3Double>();
                XmlNode poslistnode = ringnode.SelectSingleNode("gml:posList", nsmr);
                if (poslistnode != null)
                {
                    List<Vector2> uv = null;
                    if (parameterizedTexture != null)
                    {
                        uv = parameterizedTexture.targets[ringnode.Attributes["gml:id"].Value];
                    }
                    XmlAttribute idattribute = ringnode.Attributes["gml:id"];
                    if (idattribute != null) { string id = ringnode.Attributes["gml:id"].Value; }
                    ExteriorRings.Add(new LinearRing(ReadLinearRing(poslistnode), id,uv));
                }
            }
        }
        private List<Vector3Double> ReadLinearRing(XmlNode node)
        {
            List<Vector3Double> poscoords = new List<Vector3Double>();
            string posliststring = node.InnerText.Trim(' ');
            string[] poslist = posliststring.Split(' ');
            Vector3Double coord;
            for (int i = 0; i < poslist.Length-1; i += 3)
            {
                coord = new Vector3Double();
                coord.x = double.Parse(poslist[i], System.Globalization.CultureInfo.InvariantCulture);
                coord.y = double.Parse(poslist[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                coord.z = double.Parse(poslist[i + 2], System.Globalization.CultureInfo.InvariantCulture);
                poscoords.Add(coord);

            }

            return poscoords;
        }
    }

    public class SolidProperty
    {
        XmlNode SolidNode;

        public SolidProperty(XmlNode node)
        {
            SolidNode = node;
        }
        // in-line geometry

        //href-geometry in BoundedBy


    }
}