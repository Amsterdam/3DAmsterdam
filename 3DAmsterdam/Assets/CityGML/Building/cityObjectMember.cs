using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using ConvertCoordinates;

public class cityObjectMember
{
    XmlNamespaceManager nsmr;

    public string name;
    public string creationdate;
    private XmlNode Buildingnode;
    public List<string> RequiredSurfaceIDs = new List<string>();
    public Dictionary<string, BuildingSurface> BuildingSurfaces = new Dictionary<string, BuildingSurface>();

    public cityObjectMember(XmlNode node,XmlNamespaceManager nsmgr)
    {
        nsmr = nsmgr;
        Buildingnode = node.SelectSingleNode("bldg:Building", nsmr);
        XmlNode namenode = Buildingnode.SelectSingleNode("gml:name", nsmr);
        if (namenode != null) { name = namenode.InnerText; }
        XmlNode creationdatenode = Buildingnode.SelectSingleNode("core:creationDate",nsmr);
        if (creationdate !=null){creationdate = creationdatenode.InnerText;}  
        XmlNode lod2solidnode = Buildingnode.SelectSingleNode("bldg:lod2Solid", nsmr);
        XmlNodeList reqSurfaceMembers = lod2solidnode.SelectNodes("gml:Solid/gml:exterior/gml:CompositeSurface/gml:surfaceMember", nsmr);
        foreach (XmlNode reqSurfaceMember in reqSurfaceMembers)
        {
            string idstring = reqSurfaceMember.Attributes["xlink:href"].Value;
            RequiredSurfaceIDs.Add(idstring.Substring(1));
        }
        XmlNodeList BoundedBynodes = Buildingnode.SelectNodes("bldg:boundedBy", nsmr);
        foreach (XmlNode BoundedByNode in BoundedBynodes)
        {
            ReadboundedByNode(BoundedByNode);
        }


    }
    private void ReadboundedByNode(XmlNode node)
    {
        XmlNode surfacenode = node.FirstChild;
        XmlNode surfacenamenode = surfacenode.SelectSingleNode("gml:name", nsmr);
        XmlNode creationdatenode = surfacenode.SelectSingleNode("core:creationDate", nsmr);

        XmlNodeList SurfacememberNodes = surfacenode.SelectNodes("bldg:lod2MultiSurface/gml:MultiSurface/gml:surfaceMember", nsmr);

        foreach (XmlNode SurfacememberNode in SurfacememberNodes)
        {
            BuildingSurface surface = new BuildingSurface();
            surface.Surfacetype = surfacenode.LocalName;
            if (surfacenamenode != null) { surface.name = surfacenamenode.InnerText; }
            if (creationdate != null) { surface.creationdate = creationdatenode.InnerText; }
            XmlNode CompositeSurfaceNode = SurfacememberNode.SelectSingleNode("gml:CompositeSurface", nsmr);
            if (CompositeSurfaceNode != null)
            {
                surface.SurfaceID = CompositeSurfaceNode.Attributes["gml:id"].Value;
                surface.triangles = ReadCompositeSurface(CompositeSurfaceNode);
                BuildingSurfaces.Add(surface.SurfaceID, surface);
            }
            XmlNode LinearRingnode = SurfacememberNode.SelectSingleNode("gml:Polygon", nsmr);
            if (LinearRingnode != null)
            {
                surface.SurfaceID = LinearRingnode.Attributes["gml:id"].Value;
                surface.triangles = ReadPolygonSurface(LinearRingnode);
                BuildingSurfaces.Add(surface.SurfaceID, surface);
            }

            
        }

    }

    private List<Triangle> ReadCompositeSurface(XmlNode node)
    {
        List<Triangle> tris = new List<Triangle>();
        XmlNodeList polygonNodes = node.SelectNodes("gml:surfaceMember/gml:Polygon", nsmr);
        foreach (XmlNode polygonNode in polygonNodes)
        {
            XmlNode poslist = polygonNode.SelectSingleNode("gml:exterior/gml:LinearRing/gml:posList", nsmr);
            string posliststring = poslist.InnerText;
            string[] coords = posliststring.Split(' ');
            Triangle tri = new Triangle();
            for (int i = 3; i < coords.Length; i+=3)
            {
                Vector3RD coord = new Vector3RD();
                coord.x = double.Parse(coords[i], System.Globalization.CultureInfo.InvariantCulture);
                coord.y = double.Parse(coords[i+1], System.Globalization.CultureInfo.InvariantCulture);
                coord.z = double.Parse(coords[i+2], System.Globalization.CultureInfo.InvariantCulture);
                tri.vertices.Add(coord);
            }
            tris.Add(tri);

        }

        return tris;
    }
    private List<Triangle> ReadPolygonSurface(XmlNode LinearRingnode)
    {
        List<Triangle> tris = new List<Triangle>();
        XmlNode poslist = LinearRingnode.SelectSingleNode("gml:exterior/gml:LinearRing/gml:posList", nsmr);
        string posliststring = poslist.InnerText;
        string[] coords = posliststring.Split(' ');
        List<Vector3> Vecs = new List<Vector3>();
        List<Vector3RD> VecsRD = new List<Vector3RD>();
        for (int i = 3; i < coords.Length; i += 3)
        {
            Vector3 coord = new Vector3();
            Vector3RD coordRD = new Vector3RD();
            coord.x = float.Parse(coords[i], System.Globalization.CultureInfo.InvariantCulture);
            coord.y = float.Parse(coords[i + 1], System.Globalization.CultureInfo.InvariantCulture);
            coord.z = float.Parse(coords[i + 2], System.Globalization.CultureInfo.InvariantCulture);
            Vecs.Add(coord);
            coordRD.x = double.Parse(coords[i], System.Globalization.CultureInfo.InvariantCulture);
            coordRD.y = double.Parse(coords[i + 1], System.Globalization.CultureInfo.InvariantCulture);
            coordRD.z = double.Parse(coords[i + 2], System.Globalization.CultureInfo.InvariantCulture);
            VecsRD.Add(coordRD);
        }

        Triangulator tr = new Triangulator(Vecs.ToArray());
        int[] indices = tr.Triangulate();

        
        for (int j = 0; j < indices.Length; j+=3)
        {
            Triangle newTri = new Triangle();
            newTri.vertices.Add(VecsRD[indices[j]]);
            newTri.vertices.Add(VecsRD[indices[j+1]]);
            newTri.vertices.Add(VecsRD[indices[j + 2]]);
            tris.Add(newTri);
        }


        return tris;

    }
}
public class BuildingSurface
{
    public string Surfacetype;
    public string name;
    public string creationdate;
    public string SurfaceID;
    public List<Triangle> triangles;
}
public class Triangle
{
    public List<Vector3RD> vertices = new List<Vector3RD>();
    

}
