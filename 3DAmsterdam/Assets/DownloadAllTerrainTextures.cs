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
    [SerializeField]
    private string url = "https://saturnus.geodan.nl/mapproxy/bgt/service?crs=EPSG%3A3857&service=WMS&version=1.1.1&request=GetMap&styles=&format=image%2Fjpeg&layers=bgt&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";
    [SerializeField]
    private string localTileFilesFolder = "C:\\Users\\Sam\\Desktop\\terrain";

    private Terrain.TmsGlobalGeodeticTileSchema schema;
    private int filesDownloaded = 0;
    private int fileDownloadsStarted = 0;

    private StreamWriter streamLogWriter;

    [SerializeField]
	private int maxConcurrentDownloads = 30;

    private int downloadSlots;

    [SerializeField]
    private bool writeDownloadCommandsInsteadOfFailedFiles = true;

    void Start()
    {
        schema = new Terrain.TmsGlobalGeodeticTileSchema();
        StartCoroutine(DownloadAll());

        if (writeDownloadCommandsInsteadOfFailedFiles)
        {
            streamLogWriter = File.CreateText(localTileFilesFolder + "autodownload.py");
            streamLogWriter.WriteLine("import requests");
        }
        else{
            streamLogWriter = File.CreateText(localTileFilesFolder + "log.txt");
        }
    }

    /// <summary>
    /// Downloads all the textures for the tiles, recursively, in the target folder
    /// </summary>
    /// <returns></returns>
	private IEnumerator DownloadAll()
    {
        Debug.Log("Downloading. Logging every 500 file..");
        yield return new WaitForEndOfFrame();

        string[] zDirs = Directory.GetDirectories(localTileFilesFolder);
        foreach (var zDir in zDirs)
        {
            //Use this if you only want to read a specific folder
            //if (!(Path.GetFileNameWithoutExtension(zDir) == "18")) continue;

            string[] xDirs = Directory.GetDirectories(zDir);
            foreach (var xDir in xDirs)
            {
                string[] yFiles = Directory.GetFiles(xDir);
                downloadSlots = maxConcurrentDownloads;
                foreach (var yFile in yFiles)
                {
                    

                    var tileId = new Vector3(
                       int.Parse(Path.GetFileNameWithoutExtension(xDir)),
                       int.Parse(Path.GetFileNameWithoutExtension(yFile)),
                       int.Parse(Path.GetFileNameWithoutExtension(zDir))
                     );

                    fileDownloadsStarted++;
                    var tileRange = new TileRange(UnityEngine.Mathf.RoundToInt(tileId.x), UnityEngine.Mathf.RoundToInt(tileId.y));
                    Extent subtileExtent = TileTransform.TileToWorld(tileRange, tileId.z.ToString(), schema);
                    string wmsUrl = url.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
                    if (tileId.z == 17)
                    {
                        wmsUrl = wmsUrl.Replace("width=256", "width=1024").Replace("height=256", "height=1024");
                    }

                    if (writeDownloadCommandsInsteadOfFailedFiles)
                    {
                        streamLogWriter.WriteLine("url = r\"" + wmsUrl + "\"");
                        streamLogWriter.WriteLine("downloaded_obj = requests.get(url)");
                        streamLogWriter.WriteLine("with open(r\"" + yFile.Replace(".terrain", ".jpg").Replace("terrain", "terrain_textures") + "\", \"wb\") as file:");
                        streamLogWriter.WriteLine("    file.write(downloaded_obj.content)");
                        streamLogWriter.WriteLine("print (url)");
                        filesDownloaded++;
                     }
                    else
                    {
                        while (downloadSlots <= 0)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        StartCoroutine(SeperateFileProgress(wmsUrl, yFile));
                    }
                }
                while (filesDownloaded < fileDownloadsStarted)
                {   
                    //Log some progress every 1000 frames
                    if(filesDownloaded % 500 == 0)
                        Debug.Log("Downloaded " + filesDownloaded + "/" + fileDownloadsStarted);
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForEndOfFrame();
            }
        }
        streamLogWriter.Close();
    }

    private IEnumerator SeperateFileProgress(string wmsUrl, string targetFile)
    {
        UnityWebRequest www = UnityWebRequest.Get(wmsUrl);
        yield return www.SendWebRequest();

        filesDownloaded++;
        downloadSlots++;

        if (!www.isNetworkError && !www.isHttpError)
        {
            var loadedTextureData = www.downloadHandler.data;
            var savedFile = targetFile.Replace(".terrain", ".jpg").Replace("terrain", "terrain_textures");
            if (!File.Exists(savedFile))
            {
                File.WriteAllBytes(savedFile, loadedTextureData);
            }
        }
        else
        {
            if(!writeDownloadCommandsInsteadOfFailedFiles)
                streamLogWriter.WriteLine(wmsUrl + " - " + targetFile.Replace(".terrain", ".jpg").Replace("terrain", "terrain_textures"));
        }
        yield return new WaitForEndOfFrame();
    }
}


