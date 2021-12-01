using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSessionSaver : IDataSaver
{
    public void ExportSaveData(string sessionId)
    {
        PlayerPrefs.Save();
    }

    public void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public void ClearAllData(string sessionId)
    {
        PlayerPrefs.DeleteAll();
    }
}
