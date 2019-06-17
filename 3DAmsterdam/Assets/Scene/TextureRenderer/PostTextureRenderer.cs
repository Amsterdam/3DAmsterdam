using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostTextureRenderer : MonoBehaviour
{
    string TextureName;
    int TexWidth;
    int TexHeight;
    Dictionary<string, Texture2D> Textures;

    public void BeginCapture()
    {
        Textures = new Dictionary<string, Texture2D>();
    }

    public void PreRender(string textureName, int texWidth, int texHeight)
    {
        TextureName = textureName;
        TexWidth = texWidth;
        TexHeight = texHeight;
    }

    private void OnPostRender()
    {
        if (Textures == null)
            return;

        if (Textures.ContainsKey(TextureName))
            return;

        //Create a new texture with the width and height of the screen
        Texture2D texture = new Texture2D(TexWidth, TexHeight, TextureFormat.RGB24, false);
        //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        texture.ReadPixels(new Rect(0, 0, TexWidth, TexHeight), 0, 0, false);
        texture.Apply();

        Textures.Add(TextureName, texture);
    }

    public Dictionary<string, Texture2D> EndCapture()
    {
        var res = Textures;
        Textures = null;
        return res;
    }
}
