using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public static class SessionSaver
{
    public static bool LoadPreviousSession = false;

    private static IDataLoader loader { get { return JSONSessionLoader.Instance; } }
    private static IDataSaver saver { get { return JsonSessionSaver.Instance; } } //= new JSONSessionSaver();

    //public const string JSON_SESSION_SAVE_DATA_BASE_KEY = "SessionJson";
    public static string SessionId { get; private set; }
    public static bool HasLoaded => loader.HasLoaded;
    //public static string SessionIdSaveKey { get { return JSON_SESSION_SAVE_DATA_BASE_KEY + SessionId; } }

    static SessionSaver()
    {
        //if (!LoadPreviousSession)
        //    saver.ClearAllData();
        SessionId = Application.absoluteURL.GetUrlParamValue("sessionId");
        Debug.Log("session id: " + SessionId);

        //ReadSaveData();
    }

    public static void ClearAllSaveData()
    {
        saver.ClearAllData(SessionId);
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
         saver.ExportSaveData(SessionId);
    }

    public static void ReadSaveData()
    {
        loader.ReadSaveData(SessionId);
    }
}
