using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSessionLoader : IDataLoader
{
    public bool HasLoaded => true;
    public event IDataLoader.DataLoadedEventHandler LoadingCompleted;

    public float LoadFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public int LoadInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public string LoadString(string key)
    {
        return PlayerPrefs.GetString(key);
    }

    public bool LoadBool(string key)
    {
        return PlayerPrefs.GetInt(key) > 0 ? true : false;
    }

    public void ReadSaveData(string sessionId)
    {
    }
}
