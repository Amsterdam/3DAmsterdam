#define DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
//using Netherlands3D.Sharing;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SessionSaver
{
    public static bool LoadPreviousSession { get; set; } = true;

    public static JSONSessionLoader Loader { get { return ServiceLocator.GetService<JSONSessionLoader>(); } }
    public static JsonSessionSaver Saver { get { return ServiceLocator.GetService<JsonSessionSaver>(); } }

    public static string SessionId { get; private set; }
    public static bool HasLoaded => Loader.HasLoaded;

    static SessionSaver()
    {
#if UNITY_EDITOR
        //if (SessionId == null)
        //{
        Debug.Log("Session id not found, using testID");
        //SessionId = "29f0f430-7206-11ec-9808-3117a2780adf";
        //SessionId = "2ffaf370-576d-11ec-8a04-35c209595469"; //Stadhouderslaan 79 Utrecht IFC model
        //SessionId = "4f2e5410-85af-11ec-b02f-8508d1049190"; //Stadhouderslaan 79 Utrecht
        //SessionId = "719bd3a0-8376-11ec-9953-4b875c5f9637"; // Jan Pieterzoon Coenstr 40 Utrecht
        //SessionId = "1186a670-8849-11ec-b2fe-1dc941e32fde"; //Haarlemmerstraatweg 35
        //SessionId = "ddd8af70-9e15-11ec-a02d-f7d9626e4800";
        //SessionId = "feacd4b0-9bb8-11ec-89dd-6536bf8ad53b";
        //}
        //SessionId = "17b6cfc0-a531-11ec-b88b-cff3a4fba0f3";
        //SessionId = "7e184780-a9c3-11ec-ac3e-5b3e00f4b66e";
        //SessionId = "d114e8f0-aea8-11ec-b7c3-51a5ce73359d";
        //SessionId = "831201c0-ba78-11ec-a2c7-f3c0d57d2cd6"; //Testgebouw2
        //SessionId = "4f966cc0-e8ac-11ec-a08e-bb6256c734e4"; //no snap
        //SessionId = "47f81880-bbe1-11ec-bb7e-b7bbd72d450c"; //snap
        //SessionId = "961bc940-bfd7-11ec-8a43-ddfbd3cd7e77"; //shapeable
        //SessionId = "5c66b840-cae5-11ec-9fe1-e30841db76e6"; // uploaded
        //SessionId = "a1fd6ee0-cf8a-11ec-8235-9b9afe916f92"; //JSON upload test
        //SessionId = "b9c68c40-d201-11ec-bab9-853726a718cd"; //ifc converter bug (nosnap, upload flow)
        //SessionId = "2029d500-e738-11ec-b6ff-d190c2e036a3";
        //SessionId = "2e614640-ebce-11ec-b22e-81bc97112413";

        //SessionId = "b1c02c00-f7b7-11ec-bc3d-6feff600655c"; //Johan Gramstraat 83 Den Haag
        //SessionId = "e8094d50-fde3-11ec-90bd-6d0b17474de4"; //uploaded uitbouw test
        //SessionId = "fd387d20-0835-11ed-b7de-dbf64ddb94ff"; //new export with sketchup testfile, no snap
        //SessionId = "9924a050-0ce2-11ed-b2e1-6fd693bef9a8";//new export with sketchup testfile, with snap
        //SessionId = "e5f0dc50-13d6-11ed-aa77-8595e0614b2f"; //annotation only
        //SessionId = "5c0827e0-13ff-11ed-be89-71de1e5c51ce"; //teken uitbouw
        //SessionId = "1a21dab0-2485-11ed-a576-bd18e2523c39"; //uploaded uitbouw
        SessionId = "1311e590-2ac4-11ed-892b-5df1149f6052";

#else
        SessionId = Application.absoluteURL.GetUrlParamValue("sessionId");
#endif
        SessionId += "_html";

        Debug.Log("session id: " + SessionId);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene != ErrorService.ErrorScene)
            LoadSaveData(); //This data also includes essential information like bagId, so always load the data
    }

    public static void ClearAllSaveData()
    {
        Saver.ClearAllData(SessionId);
    }

    public static void ExportSavedData()
    {
        Saver.ExportSaveData(SessionId);
    }

    public static void UploadFileToEndpoint()
    {
        Saver.UploadCityJSONFileToEndpoint();
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
        ServiceLocator.GetService<T3DInit>().LoadBuilding();
        Loader.LoadingCompleted -= Loader_LoadingCompleted;
    }

    public static void AddContainer(SaveDataContainer saveDataContainer)
    {
        Saver.AddContainer(saveDataContainer);
    }

    public static void RemoveContainer(SaveDataContainer saveDataContainer)
    {
        Saver.RemoveContainer(saveDataContainer);
    }

    public static void LoadContainer(SaveDataContainer saveDataContainer)
    {
        //check if object already exists in the save data, in which case load the save data:
        if (ServiceLocator.GetService<JSONSessionLoader>().TryGetJson(saveDataContainer.TypeKey, saveDataContainer.InstanceId, out string json))
        {
            JsonUtility.FromJsonOverwrite(json, saveDataContainer);
        }
    }

    public static JSONNode GetJSONNodeOfType(string typeKey)
    {
        return Loader.GetJSONNodeOfType(typeKey);
    }
}
