using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System;

public class TextureRenderer
{
    public static Dictionary<string, Texture2D> CopyAndMakeReadable(Texture[] textures)
    {
        Dictionary<string, Texture2D> output = new Dictionary<string, Texture2D>();
        CopyAndMakeReadable2(textures).ForEach(t =>
        {
           string texturehash = MtlExporter.GetTexName(t);
           if (!output.ContainsKey(texturehash))
           {
               output.Add(texturehash, t);
           }
        });
        return output;
    }


    public static List<Texture2D> CopyAndMakeReadable2(Texture[] textures)
    {
        List<Texture2D> output = new List<Texture2D>();
        foreach (var t in textures)
        {
            RenderTexture render_texture = RenderTexture.GetTemporary(t.width, t.height, 24, RenderTextureFormat.ARGB32);
            Graphics.Blit(t, render_texture);

            RenderTexture.active = render_texture;
            Texture2D temp = new Texture2D(t.width, t.height);
            temp.ReadPixels(new Rect(0, 0, render_texture.width, render_texture.height), 0, 0);
            temp.Apply();

            output.Add(temp);
        }
        return output;
    }
}