using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSessionSaver : IDataSaver
{
    public event IDataSaver.DataSavedEventHandler SavingCompleted;
    public bool SaveInProgress => false;

    public void ExportSaveData(string sessionId)
    {
        PlayerPrefs.Save();
        SavingCompleted?.Invoke(true);
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

    public void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public void ClearAllData(string sessionId)
    {
        PlayerPrefs.DeleteAll();
    }
}
