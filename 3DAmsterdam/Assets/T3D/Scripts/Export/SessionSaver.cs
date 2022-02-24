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
            //SessionId = "29f0f430-7206-11ec-9808-3117a2780adf";
            //SessionId = "2ffaf370-576d-11ec-8a04-35c209595469"; //Stadhouderslaan 79 Utrecht IFC model
            //SessionId = "4f2e5410-85af-11ec-b02f-8508d1049190"; //Stadhouderslaan 79 Utrecht
            //SessionId = "719bd3a0-8376-11ec-9953-4b875c5f9637"; // Jan Pieterzoon Coenstr 40 Utrecht
            //SessionId = "1186a670-8849-11ec-b2fe-1dc941e32fde"; //Haarlemmerstraatweg 35
            SessionId = "dd1c1020-9557-11ec-986f-c747671180da"; 

        }
        SessionId += "_html";

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

    public static void SaveBool(string key, bool value)
    {
        Saver.SaveBool(key, value);
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

    public static bool LoadBool(string key)
    {
        return Loader.LoadBool(key);
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
