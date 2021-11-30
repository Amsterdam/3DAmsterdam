using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataSaver
{
    public void SaveFloat(string key, float value);
    public void SaveInt(string key, int value);
    public void SaveString(string key, string value);
    public void ExportSaveData(string sessionId);
    public void ClearAllData(string sessionId);
}
