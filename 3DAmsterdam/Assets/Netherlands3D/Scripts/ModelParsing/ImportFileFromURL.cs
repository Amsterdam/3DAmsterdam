using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class ImportFileFromURL : MonoBehaviour
{
    public UnityEvent<string> filesImportedEvent;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ImportFromURL(string url, string filename);
#endif
    public void Import(string url)
    {
        LoadingScreen.Instance.ShowMessage($"{url} wordt gedownload..");
        LoadingScreen.Instance.SetProgressBarNormalisedValue(0.001f);

        var filenameWithExtention = Path.GetFileName(url).Split(".")[0];

#if UNITY_WEBGL && !UNITY_EDITOR
        //Callbacks for WebGL go through FileInputIndexDB        
        ImportFromURL(url, filenameWithExtention);
#else
        StartCoroutine(DownloadAndImport(url, filenameWithExtention));    
#endif
    }

    private IEnumerator DownloadAndImport(string url, string filename)
    {
        UnityWebRequest getModelRequest = UnityWebRequest.Get(url);
        yield return getModelRequest.SendWebRequest();

        if (getModelRequest.result == UnityWebRequest.Result.Success)
        {
            var data = getModelRequest.downloadHandler.data;
            File.WriteAllBytes($"{Application.persistentDataPath}/{filename}", data);

            filesImportedEvent.Invoke(filename);
        }
        else
        {
            
        }
        yield return null;
    }
}
