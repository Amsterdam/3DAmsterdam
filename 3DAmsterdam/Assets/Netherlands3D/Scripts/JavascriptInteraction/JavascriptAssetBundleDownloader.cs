using Netherlands3D.LayerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class JavascriptAssetBundleDownloader : MonoBehaviour
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

    public static JavascriptAssetBundleDownloader Instance;

    private Dictionary<string, RawDownload> runningDownloads;

    [Serializable]
    private class RawDownload
    {
        public AssetBundle assetBundle;
        public TileChange tileChange;
        public DownloadState state;
    }

    private enum DownloadState{
        RUNNING,
        COMPLETED
	}

    private void Awake()
	{
        Instance = this;
        runningDownloads = new Dictionary<string, RawDownload>();
    }

    public IEnumerator StartAndWaitForDownload(string url, TileChange tileChange, Action<AssetBundle> retrievedAssetBundle = null)
    {
        //Start a new download
        if (!runningDownloads.ContainsKey(url))
        {
            Debug.Log("Starting JS download: " + url);
            runningDownloads.Add(url, new RawDownload() { tileChange = tileChange, state = DownloadState.RUNNING });
            DownloadFileData(url);
        }

        //Wait for it do be done or aborted
        while (runningDownloads.TryGetValue(url, out RawDownload rawDownload))
        {
            if (rawDownload.state != DownloadState.COMPLETED)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                //Finished downloading
                Debug.Log("Completed JS download: " + url);
                retrievedAssetBundle(rawDownload.assetBundle);
                runningDownloads.Remove(url);
                yield break;
            }
		}        
    }

    /// <summary>
    /// Download all running downloads with the same url
    /// </summary>
    /// <param name="tileChange"></param>
    public void FileDownloadAborted(string url)
    {
        if(url != "" && runningDownloads.ContainsKey(url)){
            AbortDownloadProgress(url);
            runningDownloads.Remove(url);
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

            runningDownloads[url].assetBundle = AssetBundle.LoadFromMemory(data);
            runningDownloads[url].state = DownloadState.COMPLETED;
            FreeFileData(url);
        }
    }

    /*private void TestParseData(ref byte[] data)
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
    }*/
}
