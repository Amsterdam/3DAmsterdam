using Netherlands3D.LayerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JavascriptDownloader : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void DownloadFileData(string fileName);
    [DllImport("__Internal")]
    private static extern void AbortDownloadProgress(string fileName);
    [DllImport("__Internal")]
    private static extern int GetFileDataLength(string fileName);
    [DllImport("__Internal")]
    private static extern IntPtr GetFileData(string fileName);
    [DllImport("__Internal")]
    private static extern int FreeFileData(string fileName);

    private string testDownloadUrl = "https://acc.3d.amsterdam.nl/web/data/develop/RD/Buildings/buildings_120000_483000.2,2";
    private string lastUrl = "";

    public static JavascriptDownloader Instance;

    private Dictionary<string, DownloadState> runningDownloads;

    private enum DownloadState{
        STARTED,
        COMPLETED
	}

    private void Awake()
	{
        Instance = this;
        runningDownloads = new Dictionary<string, DownloadState>();
    }

    public void StartDownload(string url, Action<TileChange> callback = null)
    {
        if (!runningDownloads.ContainsKey(url))
        {
            runningDownloads.Add(url, DownloadState.STARTED);
            DownloadFileData(url);
        }
    }

	public void FileDownloaded(string url)
    {
        if (runningDownloads.ContainsKey(url))
        {
            //we were expecting this data
            var length = GetFileDataLength(url);
            var ptr = GetFileData(url);
            var data = new byte[length];
            Marshal.Copy(ptr, data, 0, data.Length);

            runningDownloads.Remove(url);
        }

        
        FreeFileData(url);
    }

	private void TestParseData(ref byte[] data)
	{
        var assetBundle = AssetBundle.LoadFromMemory(data);
        Mesh[] meshesInAssetbundle = null;
        try
        {
            meshesInAssetbundle = assetBundle.LoadAllAssets<Mesh>();
        }
        catch (Exception)
        {
            assetBundle.Unload(true);
        }
        if (meshesInAssetbundle != null)
        {
           var mesh = meshesInAssetbundle[0];
           this.gameObject.AddComponent<MeshFilter>().mesh = mesh;
           this.gameObject.AddComponent<MeshRenderer>();
        }
    }

	private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastUrl = testDownloadUrl + "?random=" + UnityEngine.Random.value;
            DownloadFileData(lastUrl);

        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            AbortDownloadProgress(lastUrl);
        }
    }
}
