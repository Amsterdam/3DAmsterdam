using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


public class ParameterizedTexture
{
    public string imageURI;
    public string textureType;
    public string Wrapmode;
    public string bordercolor;
    public Dictionary<string, List<Vector2>> target = new Dictionary<string, List<Vector2>>();
}


public class AppearanceMember
{
    public List<ParameterizedTexture> textures = new List<ParameterizedTexture>();

    public AppearanceMember(XmlNode node)
    {
        foreach (XmlNode child in node.ChildNodes)
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
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
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
        textures.Add(tex);
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "imageURI":
                    tex.imageURI=child.InnerText;
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
    }
    private void ReadTarget(XmlNode node, ParameterizedTexture tex)
    {

    }
}


