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
        foreach (var t in textures)
        {
            

            RenderTexture render_texture = RenderTexture.GetTemporary(t.width, t.height, 24, RenderTextureFormat.ARGB32);
            Graphics.Blit(t, render_texture);

            RenderTexture.active = render_texture;
            Texture2D temp = new Texture2D(t.width, t.height);
            temp.ReadPixels(new Rect(0, 0, render_texture.width, render_texture.height), 0, 0);
            temp.Apply();

            string texturehash = GetTextureHash(temp);
            if (output.ContainsKey(texturehash))
                continue;

            output.Add(texturehash, temp);
        }
        return output;
    }
    private static string GetTextureHash(Texture2D tex)
    {
        Color32[] texCols = tex.GetPixels32();
        byte[] rawTextureData = Color32ArrayToByteArray(texCols);
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] hashbytes = md5.ComputeHash(rawTextureData);

        StringBuilder sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < hashbytes.Length; i++)
        {
            sBuilder.Append(hashbytes[i].ToString("x2"));
        }
        string hash = sBuilder.ToString();
        return hash;
    }
    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }
}