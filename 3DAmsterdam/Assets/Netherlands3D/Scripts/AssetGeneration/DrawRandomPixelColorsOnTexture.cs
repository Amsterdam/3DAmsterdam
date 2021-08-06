/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
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
