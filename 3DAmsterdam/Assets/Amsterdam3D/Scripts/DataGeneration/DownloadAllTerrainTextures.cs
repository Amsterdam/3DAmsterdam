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
    private bool generateDownloadList = true;

    private const string downloadList = "_download_list.txt";
    private const string failedUnityDownloads = "_failed_unity_downloads.txt";

    protected StreamReader reader = null;
    protected string readLine = "";

    private int maxLinesPerFrame = 1000;
	private bool overwrite = false;

	private void Start()
	{
        schema = new Terrain.TmsGlobalGeodeticTileSchema();
    }

	[ContextMenu("Download all texture files")]
    public void DownloadAllTextureFiles()
    {
        
        generateDownloadList = false;
        streamLogWriter = File.CreateText(localTileFilesFolder + failedUnityDownloads);
        StartCoroutine(DownloadAll());
    }

   [ContextMenu("Only create a download list")]
    public void WriteAllDownloadUrlsToList()
    {
        generateDownloadList = true;
        streamLogWriter = File.CreateText(localTileFilesFolder + downloadList);
        StartCoroutine(DownloadAll());
    }

    /// <summary>
    /// Downloads all the textures for the tiles, recursively, in the target folder
    /// </summary>
    /// <returns></returns>
	private IEnumerator DownloadAll()
    {
        Debug.Log("Working....");
        yield return new WaitForEndOfFrame();

        string[] zDirs = Directory.GetDirectories(localTileFilesFolder);
        foreach (var zDir in zDirs)
        {
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

                    if (generateDownloadList)
                    {
                        streamLogWriter.WriteLine(wmsUrl + " " + yFile.Replace(".terrain", ".jpg").Replace("terrain", "terrain_textures"));
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
        Debug.Log("Done");
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
            if(!generateDownloadList)
                streamLogWriter.WriteLine(wmsUrl + " - " + targetFile.Replace(".terrain", ".jpg").Replace("terrain", "terrain_textures"));
        }
        yield return new WaitForEndOfFrame();
    }

    [ContextMenu("Download all urls in text list to files")]
    private void DownloadAllFromList(){
        StartCoroutine(DownloadListedURLs());
	}
    private IEnumerator DownloadListedURLs(){
        if (!File.Exists(localTileFilesFolder + downloadList)) {
            Debug.Log(localTileFilesFolder + downloadList + " not there. Please generate, or add it first.");
            yield break;
        }
        
        FileInfo downloadListFileInfo = new FileInfo(localTileFilesFolder + downloadList);
        reader = downloadListFileInfo.OpenText();

        int downloadsDone = 0;
        int downloadsFailed = 0;
        int downloadsStarted = 0;
        while (readLine != null)
        {
            for (int i = 0; i < maxLinesPerFrame; i++)
            {
                readLine = reader.ReadLine();
                if(readLine != null)
                {
                    string[] filePaths = readLine.Split(' ');
                    if (overwrite || !File.Exists(filePaths[1]))
                    {
                        downloadsStarted++;
                        UnityWebRequest www = UnityWebRequest.Get(filePaths[0]);
                        yield return www.SendWebRequest();
                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            downloadsDone++;
                            var loadedTextureData = www.downloadHandler.data;
                            if (www.downloadHandler.data.Length > 15)
                            {
                                File.WriteAllBytes(filePaths[1], loadedTextureData);
                            }
                            else{
                                downloadsFailed++;
                            }
                        }
                        else {
                            downloadsFailed++;
                        }
                    }
                    if (downloadsDone % maxLinesPerFrame == 0)
                    {
                        Debug.Log(downloadsStarted + " downloads started." + downloadsDone + " downloaded. " + downloadsFailed + " failed.");
                        yield return new WaitForEndOfFrame();
                    }
                }
                else{
                    break;
				}
            }
        }
        Debug.Log("All lines read.");
        yield return new WaitForEndOfFrame();
    }
}


