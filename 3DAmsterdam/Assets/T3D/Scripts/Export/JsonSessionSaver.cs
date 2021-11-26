using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class JSONSessionSaver : IDataSaver
{
    public const string JSON_SESSION_SAVE_DATA_KEY = "SessionJson";
    private JSONObject rootObject = new JSONObject();

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
        //PlayerPrefs.SetString(rootObject.ToString());
    }

    public void ClearAllData()
    {
        rootObject = new JSONObject();
        PlayerPrefs.DeleteKey(JSON_SESSION_SAVE_DATA_KEY);
    }
}
