using ConvertCoordinates;
using Netherlands3D.Interface;
using Netherlands3D.LayerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColladaCreation : ModelFormatCreation
{
    [SerializeField]
    private LoadingScreen loadingScreen;
    private MeshClipper.RDBoundingBox boundingbox;

    public void CreateCollada(Bounds UnityBounds, List<Layer> layerList)
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
        ColladaFile colladaFile = new ColladaFile();

        yield return null;
        MeshClipper meshClipper = new MeshClipper();

        loadingScreen.ShowMessage("Collada-bestand genereren...");
        loadingScreen.ProgressBar.Percentage(0f);

        int layercounter = 0;
        foreach (var layer in layerList)
        {
            List<GameObject> gameObjectsToClip = GetTilesInLayer(layer, bottomLeftRD, topRightRD);
            if (gameObjectsToClip.Count == 0)
            {
                continue;
            }
            foreach (var gameObject in gameObjectsToClip)
            {
                meshClipper.SetGameObject(gameObject);
                for (int submeshID = 0; submeshID < gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount; submeshID++)
                {
                    meshClipper.clipSubMesh(boundingbox, submeshID);
                    string layerName = gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID].name.Replace(" (Instance)", "");

                    colladaFile.AddObject(meshClipper.clippedVerticesRD, layerName, gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID].GetColor("_BaseColor"));
                    yield return null;
                }
            }
            loadingScreen.ProgressBar.Percentage((float)layercounter / (float)layerList.Count);
            loadingScreen.ProgressBar.SetMessage(layer.name + "...");
            layercounter++;
        }
        FreezeLayers(layerList, false);
        colladaFile.Save();
        loadingScreen.Hide();
        Debug.Log("file saved");
    }
}
