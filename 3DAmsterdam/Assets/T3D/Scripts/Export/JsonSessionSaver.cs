using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using UnityEngine.Networking;
using Netherlands3D.Sharing;

public class JsonSessionSaver : MonoBehaviour, IDataSaver
{
    public const string JSON_SESSION_SAVE_DATA_KEY = "SessionJson";
    private JSONObject rootObject = new JSONObject();
    const string uploadURL = "https://t3dapi.azurewebsites.net/api/upload/";

    public static JsonSessionSaver Instance;
    Guid sessionId;

    private void Awake()
    {
        Instance = this;
        sessionId = Guid.NewGuid();
    }

    public void SaveFloat(string key, float value)
    {
        rootObject[key] = value;
    }

    public void SaveInt(string key, int value)
    {
        rootObject[key] = value;
    }

    public void SaveString(string key, string value)
    {
        rootObject[key] = value;
    }

    public void ExportSaveData()
    {
        Debug.Log("Saving data: " + rootObject.ToString());
        PlayerPrefs.SetString(JSON_SESSION_SAVE_DATA_KEY, rootObject.ToString());

        StartCoroutine(UploadData(sessionId.ToString(), rootObject.ToString()));
        //PlayerPrefs.SetString(rootObject.ToString());
    }

    private IEnumerator UploadData(string name, string data)
    {
        var uwr = UnityWebRequest.Put(uploadURL + name, data);
        print(uploadURL + name);

        using (uwr)
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                print("saving failed");
                print(uwr.error);
            }
            else
            {
                print("saving succeeded");
            }
        }

        //using (UnityWebRequest uwr = new UnityWebRequest(uploadURL))
        //{
        //    uwr.SetRequestHeader();
        //    yield return uwr.SendWebRequest();

        //    if (uwr.isNetworkError || uwr.isHttpError)
        //    {
        //        callback(tileChange);
        //    }
        //    else
        //    {
        //        ObjectData objectMapping = newTile.AddComponent<ObjectData>();
        //        AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
        //        data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];

        //        objectMapping.highlightIDs = oldObjectMapping.highlightIDs;
        //        objectMapping.hideIDs = oldObjectMapping.hideIDs;
        //        objectMapping.ids = data.ids;
        //        objectMapping.uvs = data.uvs;
        //        objectMapping.vectorMap = data.vectorMap;
        //        objectMapping.mesh = newTile.GetComponent<MeshFilter>().sharedMesh;
        //        objectMapping.ApplyDataToIDsTexture();
        //        newAssetBundle.Unload(true);
        //    }
        //}
    }

    public void ClearAllData()
    {
        rootObject = new JSONObject();
        PlayerPrefs.DeleteKey(JSON_SESSION_SAVE_DATA_KEY);
    }
}
