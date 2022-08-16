using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;

[Serializable]
public abstract class SaveDataContainer
{
    /// <summary>
    /// structure of the json:
    /// "TypeKey": // the type key is the type of the class that is derived from this container (as a string).
    /// {
    ///     "InstanceId": // the instance id of this container in case there are multiple containers per class. In case there is only one instance, this key defaults to "instance", similar to a singleton pattern
    ///     {
    ///         // fields are defined in the inherited classes and define the variables that need to be saved.
    ///         "Field1": "Value1" 
    ///         "Field2": "Value2"
    ///         "Field3": "Value3"
    ///     }
    /// }
    /// </summary>

    public string TypeKey { get; private set; }
    public string InstanceId { get; protected set; }

    public SaveDataContainer()
    {
        TypeKey = GetTypeKey();
        InstanceId = "instance";

        Initialize();
    }

    private void Initialize()
    {
        if (SessionSaver.HasLoaded)
        {
            LoadData();
        }

        SessionSaver.Loader.LoadingCompleted += Loader_LoadingCompleted;
        SessionSaver.AddContainer(this); //add container to save data
    }

    private void Loader_LoadingCompleted(bool loadSucceeded)
    {
        if (loadSucceeded)
            LoadData();
    }

    private void LoadData()
    {
        SessionSaver.LoadContainer(this);
    }

    public SaveDataContainer(int instanceId)
    {
        TypeKey = GetTypeKey();
        InstanceId = instanceId.ToString();
        Initialize();
    }

    public SaveDataContainer(string instanceId)
    {
        TypeKey = GetTypeKey();
        InstanceId = instanceId;
        Initialize();
    }

    private string GetTypeKey()
    {
        return GetType().ToString();
    }

    public void DeleteSaveData()
    {
        SessionSaver.RemoveContainer(this);
    }

    ~SaveDataContainer()
    {
        DeleteSaveData();
        SessionSaver.Loader.LoadingCompleted -= Loader_LoadingCompleted;
    }
}
