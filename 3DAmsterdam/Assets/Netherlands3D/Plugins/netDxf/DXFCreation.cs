using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using Netherlands3D.Interface;

public class DXFCreation : MonoBehaviour
{
    [SerializeField]
    private LoadingScreen loadingScreen;
    private MeshClipper.RDBoundingBox boundingbox;
    // Start is called before the first frame update
    void Start()
    {
        //List<Vector3RD> verts = new List<Vector3RD>();
        //verts.Add(new Vector3RD(0, 0, 0));
        //verts.Add(new Vector3RD(0, 1, 0));
        //verts.Add(new Vector3RD(0, 0, 1));
        //verts.Add(new Vector3RD(0, 0, 0));
        //verts.Add(new Vector3RD(0, 1, 0));
        //verts.Add(new Vector3RD(0, 0, 1));
        //verts.Add(new Vector3RD(0, 0, 0));
        //verts.Add(new Vector3RD(0, 1, 0));
        //verts.Add(new Vector3RD(0, 0, 1));

        //DxfFile file = new DxfFile();
        //file.SetupDXF();
        //file.AddLayer(verts,"testlaag");
        //file.Save();
    }

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
        DxfFile file = new DxfFile();
        file.SetupDXF();
        yield return null;
        MeshClipper meshClipper = new MeshClipper();
        
        loadingScreen.ShowMessage("DXF-bestand genereren...");
        loadingScreen.ProgressBar.SetMessage("");
        loadingScreen.ProgressBar.Percentage(0f);
        
        int layercounter = 0;
        foreach (var layer in layerList)
        {
            List<GameObject> gameObjectsToClip = GetTilesInLayer(layer, bottomLeftRD, topRightRD);
            if (gameObjectsToClip.Count==0)
            {
                continue;
            }
            foreach (var gameObject in gameObjectsToClip)
            {
                meshClipper.SetGameObject(gameObject);
                for (int submeshID = 0; submeshID < gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount; submeshID++)
                {
                    meshClipper.clipSubMesh(boundingbox, submeshID);
                    string layerName = gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID].name.Replace(" (Instance)","");
                    
                    file.AddLayer(meshClipper.clippedVerticesRD, layerName,GetColor(gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID]));
                    yield return null;
                }
            }

            layercounter++;
            loadingScreen.ProgressBar.Percentage((float)layercounter /(float)layerList.Count);
            loadingScreen.ProgressBar.SetMessage(layer.name + "...");
            yield return new WaitForEndOfFrame();
        }
        FreezeLayers(layerList, false);
        file.Save();
        loadingScreen.Hide();
        Debug.Log("file saved");
    }

    private void FreezeLayers(List<Layer> layerList, bool freeze)
    {
        foreach (var layer in layerList)
        {
            layer.pauseLoading = freeze;
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

            if (tileX+tilesize < bottomLeftRD.x || tileX > topRightRD.x)
            {
                continue;
            }
            if (tileY+tilesize<bottomLeftRD.y || tileY > topRightRD.y)
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
