using ConvertCoordinates;
using Netherlands3D.Interface;
using Netherlands3D.LayerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColladaCreation : MonoBehaviour
{
    [SerializeField]
    private LoadingScreen loadingScreen;
    private MeshClipper.RDBoundingBox boundingbox;

    public void CreateCollada(Bounds UnityBounds, List<Layer> layerList)
    {
        StartCoroutine(CreateFile(UnityBounds, layerList));
    }

    private void FreezeLayers(List<Layer> layerList, bool freeze)
    {
        foreach (var layer in layerList)
        {
            layer.pauseLoading = freeze;
        }
    }

    private IEnumerator CreateFile(Bounds UnityBounds, List<Layer> layerList)
    {
        FreezeLayers(layerList, true);
        Debug.Log(layerList.Count);
        Vector3RD bottomLeftRD = CoordConvert.UnitytoRD(UnityBounds.min);
        Vector3RD topRightRD = CoordConvert.UnitytoRD(UnityBounds.max);
        boundingbox = new MeshClipper.RDBoundingBox(bottomLeftRD.x, bottomLeftRD.y, topRightRD.x, topRightRD.y);
        DxfFile file = new DxfFile();
        file.SetupDXF();
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

                    //file.AddLayer(meshClipper.clippedVerticesRD, layerName, GetColor(gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID]));
                    yield return null;
                }
            }
            loadingScreen.ProgressBar.Percentage(50 * layercounter / layerList.Count);
            layercounter++;
        }
        FreezeLayers(layerList, false);
        file.Save();
        loadingScreen.Hide();
        Debug.Log("file saved");
    }

    public List<GameObject> GetTilesInLayer(Layer layer, Vector3RD bottomLeftRD, Vector3RD topRightRD)
    {
        if (layer == null)
        {
            return new List<GameObject>();
        }
        List<GameObject> output = new List<GameObject>();
        double tilesize = layer.tileSize;
        Debug.Log(tilesize);
        int tileX;
        int tileY;
        foreach (var tile in layer.tiles)
        {
            tileX = tile.Key.x;
            tileY = tile.Key.y;

            if (tileX + tilesize < bottomLeftRD.x || tileX > topRightRD.x)
            {
                continue;
            }
            if (tileY + tilesize < bottomLeftRD.y || tileY > topRightRD.y)
            {
                continue;
            }
            //if (tile.Value.gameObject.GetComponent<MeshFilter>()!=null)
            //{
            //    output.Add(tile.Value.gameObject);
            //}
            MeshFilter[] meshFilters = tile.Value.gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                output.Add(meshFilter.gameObject);
            }


        }
        return output;
    }
}
