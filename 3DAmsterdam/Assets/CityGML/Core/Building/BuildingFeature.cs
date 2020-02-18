using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
namespace CityGML
{
    public class BuildingFeature
    {

        public List<Surface> Surfaces;
        public List<AttributeData> Attributes = new List<AttributeData>();

        private XmlNode BuildingFeatureNode;
        private XmlNamespaceManager nsmr;
        public string id;
        private MultiSurfaceProperty Lod0Footprint;
        private MultiSurfaceProperty Lod0Roofedge;
        private Dictionary<int, SolidProperty> LodXSolid = new Dictionary<int, SolidProperty>();
        private Dictionary<int, MultiSurfaceProperty> LodXMultiSurface = new Dictionary<int, MultiSurfaceProperty>();
        public AppearanceMember Appearancemembers;

        //private List<string> ConsistsOfBuildingParts;

        private Dictionary<string, Surface> LinkedSurfaces = new Dictionary<string, Surface>();

        private List<string> CityGMLBoundarySurfaceThemes = new List<string>()
    { "RoofSurface",
    "WallSurface",
    "GroundSurface",
    "OuterCeilingSurface",
    "OuterFloorSurface",
    "ClosureSurface",
    "FloorSurface",
    "InteriorWallSurface",
    "CeilingSurface"
    };

        public BuildingFeature(XmlNode node, XmlNamespaceManager namespaceManager, string texturepathbase)
        {

            Appearancemembers = new AppearanceMember(node, namespaceManager, texturepathbase);
            Attributes = ReadAttributes(node);
            nsmr = namespaceManager;
            id = node.Attributes["gml:id"].Value;
            BuildingFeatureNode = node;
            foreach (XmlNode child in node.ChildNodes)
            {
                string localname = child.LocalName;
                switch (localname)
                {
                    case "lod0FootPrint":
                        Lod0Footprint = new MultiSurfaceProperty(child, nsmr);
                        break;
                    case "lod0RoofEdge":
                        Lod0Roofedge = new MultiSurfaceProperty(child, nsmr);
                        break;
                    case "lod1Solid":
                        LodXSolid.Add(1, new SolidProperty(child));
                        break;
                    case "lod2Solid":
                        LodXSolid.Add(2, new SolidProperty(child));
                        break;
                    case "lod3Solid":
                        LodXSolid.Add(3, new SolidProperty(child));
                        break;
                    case "lod4Solid":
                        LodXSolid.Add(4, new SolidProperty(child));
                        break;
                    case "lod1MultiSurface":
                        LodXMultiSurface.Add(1, new MultiSurfaceProperty(child, nsmr));
                        break;
                    case "lod2MultiSurface":
                        LodXMultiSurface.Add(2, new MultiSurfaceProperty(child, nsmr));
                        break;
                    case "lod3MultiSurface":
                        LodXMultiSurface.Add(3, new MultiSurfaceProperty(child, nsmr));
                        break;
                    case "lod4MultiSurface":
                        LodXMultiSurface.Add(1, new MultiSurfaceProperty(child, nsmr));
                        break;
                    default:
                        break;
                }
            }
        }

        public bool isLOD(int lod)
        {
            bool output = false;
            if (LodXSolid.ContainsKey(lod))
            {
                output = true;
            }
            if (LodXMultiSurface.ContainsKey(lod))
            {
                output = true;
            }
            if (lod == 0)
            {
                if (Lod0Footprint != null)
                {
                    output = true;
                }
            }
            return output;
        }

        public void GetSurfaces(int LOD, AppearanceMember appearancemember)
        {
            List<Surface> output = new List<Surface>();
            GetBoundedBySurfaces(LOD);
            string Nodename = "bldg:lod" + LOD + "Solid";
            XmlNode LodXSolidNode = BuildingFeatureNode.SelectSingleNode(Nodename, nsmr);
            if (LodXSolidNode != null)
            {
                XmlNodeList SurfaceMemberNodes = LodXSolidNode.SelectNodes("gml:Solid/gml:exterior/gml:CompositeSurface/gml:surfaceMember", nsmr);
                foreach (XmlNode SurfaceMemberNode in SurfaceMemberNodes)
                {

                    XmlAttribute att = SurfaceMemberNode.Attributes["xlink:href"];

                    if (att != null)
                    {
                        string linkstring;
                        linkstring = att.Value;
                        linkstring = linkstring.Substring(1); //skip first character in link (=#)
                        if (LinkedSurfaces.ContainsKey(linkstring))
                        {
                            output.Add(LinkedSurfaces[linkstring]);
                        }
                        else
                        {
                            Debug.Log("surface" + linkstring + " not found");
                        }
                    }
                    else
                    {
                        output.Add(new Surface(SurfaceMemberNode, nsmr, appearancemember));
                    }
                }
            }
            Surfaces = output;
        }

        private void GetBoundedBySurfaces(int LOD)
        {
            if (LOD == 0)
            {
                return;
            }
            XmlNodeList BoundedByNodes = BuildingFeatureNode.SelectNodes("bldg:boundedBy", nsmr);

            foreach (XmlNode BoundedByNode in BoundedByNodes)
            {
                List<AttributeData> attributes = ReadAttributes(BoundedByNode.FirstChild);// new Attributes(BoundedByNode);
                foreach (XmlNode child in BoundedByNode)
                {
                    if (CityGMLBoundarySurfaceThemes.Contains(child.LocalName))
                    {

                        GetLinkedSurface(child, LOD, child.LocalName, attributes);
                    }
                }
            }
        }
        private void GetLinkedSurface(XmlNode node, int LOD, string SurfaceTheme, List<AttributeData> att)
        {
            //find lodXMultiSurface
            XmlNode lodXMultiSurfacenode = node.SelectSingleNode("bldg:lod" + LOD + "MultiSurface", nsmr);
            if (lodXMultiSurfacenode is null) { return; } // quit if node doesn't exist
            XmlNodeList SurfaceMembers = lodXMultiSurfacenode.SelectNodes("gml:MultiSurface/gml:surfaceMember", nsmr);
            foreach (XmlNode SurfaceMember in SurfaceMembers)
            {
                Surface surf = new Surface(SurfaceMember, nsmr,Appearancemembers);
                string id = surf.id;
                surf.Attributes = att;
                surf.SurfaceTheme = SurfaceTheme;
                LinkedSurfaces.Add(id, surf);
            }
        }
        
        public List<AttributeData> ReadAttributes(XmlNode node)
        {
            List<AttributeData> output = new List<AttributeData>();
        
            XmlNodeList childnodes = node.ChildNodes;
            foreach (XmlNode child in childnodes)
            {
                if (child.Prefix == "")
                {
                    string name = child.Name;
                    output.Add(new AttributeData(name, child.InnerText));
                   
                }
                if (child.Name == "gml:name")
                {
                    output.Add(new AttributeData("name", child.InnerText));
                }
            }
            return output;
        }
    }
    

}


