using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawRandomPixelColorsOnTexture : MonoBehaviour
{
    void OnEnable()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.uv = mesh.uv2;        

        Texture2D texture = (Texture2D)GetComponent<MeshRenderer>().material.GetTexture("_HighlightMap");
        Texture2D newRandomColorsTexture = new Texture2D(texture.width, texture.height,texture.format,false);
        Graphics.CopyTexture(texture, newRandomColorsTexture);

		var pixels = newRandomColorsTexture.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
            pixels[i] = new Color(Random.value, Random.value, Random.value);
        }
        newRandomColorsTexture.SetPixels(pixels);
        newRandomColorsTexture.Apply();

        GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", newRandomColorsTexture);
    }
}
