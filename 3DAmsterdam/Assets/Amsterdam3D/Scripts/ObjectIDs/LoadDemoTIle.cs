using ConvertCoordinates;
using LayerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDemoTIle : MonoBehaviour
{
    public Material DefaultMaterial;
    // Start is called before the first frame update
    void Start()
    {
        string buildingURL = "file://E:/UnityData/Assetbundles/WebGL/BuildingData/building_117000_476000_lod2";
        TileChange tiledata = new TileChange();
        tiledata.X = 117000;
        tiledata.Y = 485000;
        tiledata.action = TileAction.Create;

        StartCoroutine(DownloadTile(buildingURL, tiledata));
    }

    private IEnumerator DownloadTile(string url, TileChange tileChange)
    {
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {

            Vector2Int tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            Tile tile;
           
                tile = new Tile();
                tile.LOD = 0;
                tile.tileKey = tileKey;
                tile.gameObject = new GameObject();
                tile.gameObject.transform.parent = gameObject.transform;
                tile.gameObject.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X, tileChange.Y));

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                if (tile.assetBundle is null)
                {

                }
                else
                {
                    tile.assetBundle.Unload(true);
                }
            }
            else
            {
                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                GameObject newTile = buildNewTile(newAssetBundle, tileChange);
                if (tileChange.action == TileAction.Downgrade)
                {
                    //layers[tileChange.layerIndex].tiles[tileKey].LOD--;
                }
                if (tileChange.action == TileAction.Upgrade)
                {
                    //layers[tileChange.layerIndex].tiles[tileKey].LOD++;
                }
                //Destroy(layers[tileChange.layerIndex].tiles[tileKey].gameObject);
                //layers[tileChange.layerIndex].tiles[tileKey].gameObject = newTile;

            }
            //activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
        }
        yield return null;

    }

    private GameObject buildNewTile(AssetBundle assetBundle, TileChange tileChange)
    {
        GameObject container = new GameObject();
        container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
        container.transform.parent = gameObject.transform;
        container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X + 500, tileChange.Y + 500));
        Material material = DefaultMaterial;
        Mesh[] meshesInAssetbundle = new Mesh[0];
        try
        {
            meshesInAssetbundle = assetBundle.LoadAllAssets<Mesh>();
        }
        catch (Exception)
        {
            Destroy(container);
            assetBundle.Unload(true);
        }

        foreach (Mesh mesh in meshesInAssetbundle)
        {
            Mesh newMesh = new Mesh();
            newMesh.indexFormat = mesh.indexFormat;
            newMesh.vertices = mesh.vertices;
            newMesh.triangles = mesh.triangles;
            newMesh.normals = mesh.normals;
            newMesh.name = mesh.name;
            GameObject subObject = new GameObject();
            subObject.transform.parent = container.transform;


            float X = tileChange.X;
            float Y = tileChange.Y;

            //positioning container
            Vector3RD hoekpunt = new Vector3RD(X, Y, 0);
            double OriginOffset = 500;
            Vector3RD origin = new Vector3RD(hoekpunt.x + OriginOffset, hoekpunt.y + OriginOffset, 0);
            Vector3 unityOrigin = CoordConvert.RDtoUnity(origin);
            subObject.transform.localPosition = Vector3.zero;
            double Rotatie = CoordConvert.RDRotation(origin);
            subObject.transform.Rotate(Vector3.up, (float)Rotatie);

            //subObject.transform.localPosition = Vector3.zero;
            subObject.AddComponent<MeshFilter>().mesh = newMesh;
            subObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
        assetBundle.Unload(true);
        return container;

    }
}
