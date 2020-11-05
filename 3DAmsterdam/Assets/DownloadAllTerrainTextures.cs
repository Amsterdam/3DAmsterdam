using BruTile;
using Terrain.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terrain.Tiles;
using UnityEngine;
using UnityEngine.Networking;

using System;
using ConvertCoordinates;
using Amsterdam3D.CameraMotion;
using UnityEngine.Rendering;

public class DownloadAllTerrainTextures : MonoBehaviour
{
    private string url = "https://saturnus.geodan.nl/mapproxy/bgt/service?crs=EPSG%3A3857&service=WMS&version=1.1.1&request=GetMap&styles=&format=image%2Fjpeg&layers=bgt&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";
    private string localTileFilesFolder = "C:\\Users\\Sam\\Desktop\\terrain";
	private Terrain.TmsGlobalGeodeticTileSchema schema;

	void Start()
    {
        schema = new Terrain.TmsGlobalGeodeticTileSchema();
        StartCoroutine(DownloadAll());
    }

    /// <summary>
    /// Downloads all the textures for the tiles, recursively, in the target folder
    /// </summary>
    /// <returns></returns>
	private IEnumerator DownloadAll()
	{
        string[] zDirs = Directory.GetDirectories(localTileFilesFolder);
        foreach(var zDir in zDirs)
        {
            string[] xDirs = Directory.GetDirectories(zDir);
            foreach (var xDir in xDirs)
            {
                string[] yFiles = Directory.GetFiles(xDir);
                foreach (var yFile in yFiles)
                {
                    var tileId = new Vector3(
                       int.Parse(Path.GetFileNameWithoutExtension(xDir)),
                       int.Parse(Path.GetFileNameWithoutExtension(yFile)),
                       int.Parse(Path.GetFileNameWithoutExtension(zDir))
                     );
                     Debug.Log(tileId);

                    var tileRange = new TileRange(UnityEngine.Mathf.RoundToInt(tileId.x), UnityEngine.Mathf.RoundToInt(tileId.y));
                    Extent subtileExtent = TileTransform.TileToWorld(tileRange, tileId.z.ToString(), schema);
                    string wmsUrl = url.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
                    if (tileId.z == 17)
                    {
                        wmsUrl = wmsUrl.Replace("width=256", "width=1024").Replace("height=256", "height=1024");
                    }
                    UnityWebRequest www = UnityWebRequest.Get(wmsUrl);
                    yield return www.SendWebRequest();

                    if (!www.isNetworkError && !www.isHttpError)
                    {
                        var loadedTextureData = www.downloadHandler.data;
                        var savedFile = yFile.Replace(".terrain",".jpg");
                        Debug.Log("Downloaded:");
                        Debug.Log(savedFile);
                        File.WriteAllBytes(savedFile, loadedTextureData);
                    }
                    else
                    {
                        Debug.LogWarning("Tile: [" + tileId.x + " " + tileId.y + "] Error loading texture data");
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }

    
        
    }
}


