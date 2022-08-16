using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataLoader
{
    public delegate void DataLoadedEventHandler(bool loadSucceeded);
    public event DataLoadedEventHandler LoadingCompleted;
    public bool HasLoaded { get; }

    public float LoadFloat(string key);
    public int LoadInt(string key);
    public string LoadString(string key);
    public bool LoadBool(string key);
    public void ReadSaveData(string sessionId);
}
