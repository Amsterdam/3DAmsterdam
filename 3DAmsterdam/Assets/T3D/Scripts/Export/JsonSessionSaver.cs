using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class JsonSessionSaver : IDataSaver
{
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
        Debug.Log(rootObject.ToString());
    }

}
