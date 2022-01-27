using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Netherlands3D.Sharing;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SessionSaver
{
    public static bool LoadPreviousSession { get; set; } = true;

    public static IDataLoader Loader { get { return JSONSessionLoader.Instance; } }
    public static IDataSaver Saver { get { return JsonSessionSaver.Instance; } }

    public static string SessionId { get; private set; }
    public static bool HasLoaded => Loader.HasLoaded;

    static SessionSaver()
    {
        SessionId = Application.absoluteURL.GetUrlParamValue("sessionId");
        if (SessionId == null)
        {
            Debug.Log("Session id not found, using testID");
            SessionId = "882ca7f0-7f72-11ec-b5de-7785f87f0fce";
            SessionId += "_html";
            //SessionId = "TestSession0";
        }

        Debug.Log("session id: " + SessionId);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadSaveData(); //This data also includes essential information like bagId, so always load the data
    }

    public static void ClearAllSaveData()
    {
        Saver.ClearAllData(SessionId);
    }

    public static void SaveFloat(string key, float value)
    {
        Saver.SaveFloat(key, value);
    }

    public static void SaveInt(string key, int value)
    {
        Saver.SaveInt(key, value);
    }

    public static void SaveString(string key, string value)
    {
        Saver.SaveString(key, value);
    }

    public static float LoadFloat(string key)
    {
        return Loader.LoadFloat(key);
    }

    public static int LoadInt(string key)
    {
        return Loader.LoadInt(key);
    }

    public static string LoadString(string key)
    {
        return Loader.LoadString(key);
    }

    public static void DeleteKey(string key)
    {
        Saver.DeleteKey(key);
    }

    public static void ExportSavedData()
    {
        Saver.ExportSaveData(SessionId);
    }

    public static void LoadSaveData()
    {
        Debug.Log("Loading save data for session: " + SessionId);
        Loader.ReadSaveData(SessionId);
        Loader.LoadingCompleted += Loader_LoadingCompleted;
    }

    private static void Loader_LoadingCompleted(bool loadSucceeded)
    {
        Debug.Log("loaded session: " + SessionId);
        T3DInit.Instance.LoadBuilding();
        Loader.LoadingCompleted -= Loader_LoadingCompleted;
    }
}
