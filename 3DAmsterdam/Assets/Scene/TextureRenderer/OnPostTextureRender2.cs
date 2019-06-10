using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPostTextureRender2 : MonoBehaviour
{
    public string TextureName;
    public int Width;
    public int Height;
    public Dictionary<string, Texture2D> Textures;

    public void Begin()
    {
        Textures = new Dictionary<string, Texture2D>();
    }

    private void OnPostRender()
    {
        if (Textures == null)
            return;

        if (Textures.ContainsKey(TextureName))
            return;

        //Create a new texture with the width and height of the screen
        Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false);
        texture.Apply();

        Textures.Add(TextureName, texture);
    }

    public void End()
    {
        Textures = null;
    }
}
