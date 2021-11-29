using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class JSONSessionLoader : IDataLoader
{
    private JSONNode rootObject;
    const string downloadURL = "https://t3dapi.azurewebsites.net/api/download/";

    public float LoadFloat(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public int LoadInt(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public string LoadString(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public void ReadSaveData()
    {
        var jsonString = PlayerPrefs.GetString(JSONSessionSaver.JSON_SESSION_SAVE_DATA_KEY);
        rootObject = JSONNode.Parse(jsonString);

        if (rootObject == null)
            rootObject = new JSONObject();
    }
}
