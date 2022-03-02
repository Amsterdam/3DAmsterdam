using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using Netherlands3D.TileSystem;
using Netherlands3D.Interface;

public class DXFCreation : ModelFormatCreation
{
    private MeshClipper.RDBoundingBox boundingbox;

	public void CreateDXF(Bounds UnityBounds, List<Layer> layerList)
    {
        StartCoroutine(CreateFile(UnityBounds, layerList));
    }

    private IEnumerator CreateFile(Bounds UnityBounds, List<Layer> layerList)
    {
        FreezeLayers(layerList, true);
        Debug.Log(layerList.Count);
        Vector3RD bottomLeftRD = CoordConvert.UnitytoRD(UnityBounds.min);
        Vector3RD topRightRD = CoordConvert.UnitytoRD(UnityBounds.max);
        boundingbox = new MeshClipper.RDBoundingBox(bottomLeftRD.x, bottomLeftRD.y, topRightRD.x, topRightRD.y);
        DxfFile dxfFile = new DxfFile();
        dxfFile.SetupDXF();
        yield return null;
        MeshClipper meshClipper = new MeshClipper();

        LoadingScreen.Instance.ShowMessage("DXF-bestand genereren...");
        LoadingScreen.Instance.ProgressBar.SetMessage("");
        LoadingScreen.Instance.ProgressBar.Percentage(0.1f);
        yield return new WaitForEndOfFrame();

        int layercounter = 0;
        foreach (var layer in layerList)
        {
            layercounter++;
            LoadingScreen.Instance.ProgressBar.Percentage((float)layercounter / ((float)layerList.Count+1));
            LoadingScreen.Instance.ProgressBar.SetMessage("Laag '" + layer.name + "' wordt omgezet...");
            yield return new WaitForEndOfFrame();

            List<GameObject> gameObjectsToClip = GetTilesInLayer(layer, bottomLeftRD, topRightRD);
            if (gameObjectsToClip.Count == 0)
            {
                AddChildMeshesToClippableObjects(layer, gameObjectsToClip);
            }
            if (gameObjectsToClip.Count == 0)
            {
                continue;
            }
            foreach (var gameObject in gameObjectsToClip)
            {
                meshClipper.SetGameObject(gameObject);
                for (int submeshID = 0; submeshID < gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount; submeshID++)
                {
                    string layerName = gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID].name.Replace(" (Instance)","");
                    layerName = layerName.Replace("=", "");
                    layerName = layerName.Replace("\\", "");
                    layerName = layerName.Replace("<", "");
                    layerName = layerName.Replace(">", "");
                    layerName = layerName.Replace("/", "");
                    layerName = layerName.Replace("?", "");
                    layerName = layerName.Replace("\"" ,"");
                    layerName = layerName.Replace(":", "");
                    layerName = layerName.Replace(";", "");
                    layerName = layerName.Replace("*", "");
                    layerName = layerName.Replace("|", "");
                    layerName = layerName.Replace(",", "");
                    layerName = layerName.Replace("'", "");

                    LoadingScreen.Instance.ProgressBar.SetMessage("Laag '" + layer.name + "' object " + layerName + " wordt uitgesneden...");
                    yield return new WaitForEndOfFrame();

                    meshClipper.ClipSubMesh(boundingbox, submeshID);
                    dxfFile.AddLayer(meshClipper.clippedVerticesRD, layerName,GetColor(gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID]));
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }

        LoadingScreen.Instance.ProgressBar.Percentage((float)layerList.Count / ((float)layerList.Count + 1));
        LoadingScreen.Instance.ProgressBar.SetMessage("Het AutoCAD DXF (.dxf) bestand wordt afgerond...");
        yield return new WaitForEndOfFrame();
        dxfFile.Save();

        LoadingScreen.Instance.Hide();
        FreezeLayers(layerList, false);
        Debug.Log("file saved");
    }

    private void AddChildMeshesToClippableObjects(Layer layer, List<GameObject> gameObjectsToClip)
    {
        if (layer.transform.childCount > 0)
        {
            MeshFilter[] meshFilterChildren = layer.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < meshFilterChildren.Length; i++)
            {
                gameObjectsToClip.Add(meshFilterChildren[i].gameObject);
            }
        }
    }

    private netDxf.AciColor GetColor(Material material)
    {
        if (material.GetColor("_BaseColor") !=null)
        {
            byte r = (byte)(material.GetColor("_BaseColor").r * 255);
            byte g = (byte)(material.GetColor("_BaseColor").g * 255);
            byte b = (byte)(material.GetColor("_BaseColor").b * 255);
            return new netDxf.AciColor(r, g, b);
        }
        else if (material.GetColor("_FresnelColorHigh") != null)

        {
            byte r = (byte)(material.GetColor("_FresnelColorHigh").r * 255);
            byte g = (byte)(material.GetColor("_FresnelColorHigh").g * 255);
            byte b = (byte)(material.GetColor("_FresnelColorHigh").b * 255);
            return new netDxf.AciColor(r, g, b);
        }
        else
        {
            return netDxf.AciColor.LightGray;
        }
    }
}
