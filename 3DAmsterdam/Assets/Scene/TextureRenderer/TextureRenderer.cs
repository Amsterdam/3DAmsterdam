using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureRenderer
{
    public static Dictionary<string, Texture2D> CopyAndMakeReadable(Texture[] textures)
    {
        Dictionary<string, Texture2D> output = new Dictionary<string, Texture2D>();
        foreach (var t in textures)
        {
            if (output.ContainsKey(t.imageContentsHash.ToString()))
                continue;

            RenderTexture render_texture = RenderTexture.GetTemporary(t.width, t.height, 24, RenderTextureFormat.ARGB32);
            Graphics.Blit(t, render_texture);

            RenderTexture.active = render_texture;
            Texture2D temp = new Texture2D(t.width, t.height);
            temp.ReadPixels(new Rect(0, 0, render_texture.width, render_texture.height), 0, 0);
            temp.Apply();

            output.Add(t.imageContentsHash.ToString(), temp);
        }
        return output;
    }
}