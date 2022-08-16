using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataSaver
{
    public delegate void DataSavedEventHandler(bool saveSucceeded);
    public event DataSavedEventHandler SavingCompleted;
    public bool SaveInProgress { get; }

    public void SaveFloat(string key, float value);
    public void SaveInt(string key, int value);
    public void SaveString(string key, string value);
    public void SaveBool(string key, bool value);
    public void DeleteKey(string key);
    public void ExportSaveData(string sessionId);
    public void ClearAllData(string sessionId);
}
