using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using Netherlands3D.LayerSystem;

public class DxfRuntimeTest : MonoBehaviour
{
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

        StartCoroutine(createFile(UnityBounds, layerList));
    }

    private IEnumerator createFile(Bounds UnityBounds, List<Layer> layerList)
    {

        Debug.Log(layerList.Count);
        Vector3RD bottomLeftRD = CoordConvert.UnitytoRD(UnityBounds.min);
        Vector3RD topRightRD = CoordConvert.UnitytoRD(UnityBounds.max);
        boundingbox = new MeshClipper.RDBoundingBox(bottomLeftRD.x, bottomLeftRD.y, topRightRD.x, topRightRD.y);
        DxfFile file = new DxfFile();
        file.SetupDXF();
        yield return null;
        MeshClipper meshClipper = new MeshClipper();

        foreach (var layer in layerList)
        {
            List<GameObject> gameObjectsToClip = getTilesInLayer(layer, bottomLeftRD, topRightRD);
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
                    string layerName = gameObject.GetComponent<MeshRenderer>().materials[submeshID].name;
                    
                    file.AddLayer(meshClipper.clippedVerticesRD, layerName);
                    yield return null;
                }
            }

        }
        file.Save();
        Debug.Log("file saved");
    }

    public List<GameObject> getTilesInLayer(Layer layer, Vector3RD bottomLeftRD, Vector3RD topRightRD)
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
            output.Add(tile.Value.gameObject);
        }
        return output;
    }
    
}
