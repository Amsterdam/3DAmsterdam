using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


public class ParameterizedTexture
{
    public string id;
    public string imageURI;
    public string textureType;
    public string Wrapmode;
    public string bordercolor;
    public string TargetPolygonID;
    public Dictionary<string, List<Vector2>> targets = new Dictionary<string, List<Vector2>>();
}


public class AppearanceMember
{
    public Dictionary<string, ParameterizedTexture> parametrizedTextures = new Dictionary<string, ParameterizedTexture>();
    private string Pathbase;
    public AppearanceMember(XmlNode node, XmlNamespaceManager nsmr, string TexturePathBase)
    {
        Pathbase = TexturePathBase;
        foreach (XmlNode child in node.SelectNodes("app:appearance/app:Appearance", nsmr))
        {
            switch (child.LocalName)
            {
                case "Appearance":
                    ReadAppearance(child);
                    break;
                default:
                    break;
            }
        }
    }

    private void ReadAppearance(XmlNode node)
    {
        string theme;
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "theme":
                    theme = child.InnerText;
                    break;
                case "surfaceDataMember":
                    ReadSurfaceDataMember(child);
                    break;
                default:
                    break;
            }
        }
    }

    private void ReadSurfaceDataMember(XmlNode node)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "ParameterizedTexture":
                    ReadParameterizedTexture(child);
                    break;
                default:
                    break;
            }
        }
    }
    private void ReadParameterizedTexture(XmlNode node)
    {
        ParameterizedTexture tex = new ParameterizedTexture();

        //get gml:id 
        tex.id = node.Attributes["gml:id"].Value;

        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "imageURI":
                    tex.imageURI= Pathbase+child.InnerText;
                    break;
                case "textureType":
                    tex.textureType = child.InnerText;
                    break;
                case "wrapMode":
                    tex.Wrapmode = child.InnerText;
                    break;
                case "borderColor":
                    tex.bordercolor = child.InnerText;
                    break;
                case "target":
                    ReadTarget(child,tex);
                    break;
                default:
                    break;
            }
        }
        parametrizedTextures.Add(tex.TargetPolygonID,tex);
    }


    private void ReadTarget(XmlNode node, ParameterizedTexture tex)
    {
        string targetpolygon = node.Attributes["uri"].Value;
        targetpolygon = targetpolygon.Substring(1);
        tex.TargetPolygonID = targetpolygon;

        foreach (XmlNode TexCoordList in node.ChildNodes)
        {
            foreach (XmlNode TextureCoordinates in TexCoordList)
            {
                string target = TextureCoordinates.Attributes["ring"].Value;
                target = target.Substring(1);
                tex.targets.Add(target, ReadVector2List(TextureCoordinates.InnerText));
            }
        }
    }

    private List<Vector2> ReadVector2List(string posliststring)
    {
        List<Vector2> output = new List<Vector2>();
        string[] poslist = posliststring.Split(' ');

        Vector2 coord;
        for (int i = 0; i < poslist.Length; i += 2)
        {
            coord = new Vector2();
            coord.x = float.Parse(poslist[i], System.Globalization.CultureInfo.InvariantCulture);
            coord.y = float.Parse(poslist[i + 1], System.Globalization.CultureInfo.InvariantCulture);
            output.Add(coord);
        }
        return output;
    }
}


