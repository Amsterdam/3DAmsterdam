using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public string InstanceId { get; private set; }

    public SaveDataContainer()
    {
        TypeKey = GetTypeKey();
        InstanceId = "instance";
        SessionSaver.AddContainer(this); //todo: Addcontainer needs to load data
    }

    public SaveDataContainer(int instanceId)
    {
        TypeKey = GetTypeKey();
        InstanceId = instanceId.ToString();
        SessionSaver.AddContainer(this);
    }

    public SaveDataContainer(string instanceId)
    {
        TypeKey = GetTypeKey();
        InstanceId = instanceId;
        SessionSaver.AddContainer(this);
    }

    private string GetTypeKey()
    {
        return GetType().ToString();
    }
}
