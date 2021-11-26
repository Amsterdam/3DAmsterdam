using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public static class SessionSaver
{
    public static bool LoadPreviousSession = false;

    private static IDataLoader loader = new JSONSessionLoader();
    private static IDataSaver saver = new JSONSessionSaver();

    //public static Dictionary<string, object> KeyOwners = new Dictionary<string, object>();

    static SessionSaver()
    {
        //if (!LoadPreviousSession)
        //    saver.ClearAllData();

        ReadSaveData();
    }

    public static void ClearAllSaveData()
    {
        saver.ClearAllData();
    }

    public static void SaveFloat(string key, float value)
    {   
        saver.SaveFloat(key, value);
    }

    public static void SaveInt(string key, int value)
    {
        saver.SaveInt(key, value);
    }

    public static void SaveString(string key, string value)
    {
        saver.SaveString(key, value);
    }

    public static float LoadFloat(string key)
    {
        return loader.LoadFloat(key);
    }

    public static int LoadInt(string key)
    {
        return loader.LoadInt(key);
    }

    public static string LoadString(string key)
    {
        return loader.LoadString(key);
    }

    public static void ExportSavedData()
    {
         saver.ExportSaveData();
    }

    public static void ReadSaveData()
    {
        loader.ReadSaveData();
    }
}
