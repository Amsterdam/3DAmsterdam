using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataLoader
{
    public float LoadFloat(string key);
    public int LoadInt(string key);
    public string LoadString(string key);
    public void ReadSaveData(string sessionId);
}
