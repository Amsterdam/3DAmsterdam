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
    /// "BaseKey": // the base key is the class that declared this container.
    /// {
    ///     "Id": // the id is the instance id of this container, in case there are multiple containers per class
    ///     {
    ///         // fields are defined in the inherited classes and define the variables that need to be saved.
    ///         "Field1": "Value1" 
    ///         "Field2": "Value2"
    ///         "Field3": "Value3"
    ///     }
    /// }
    /// </summary>

    public string BaseKey { get; private set; }
    public string Id { get; private set; }

    public SaveDataContainer()
    {
        BaseKey = GetBaseKey();
        Id = "0";
        SessionSaver.AddContainer(this);
    }

    public SaveDataContainer(int id)
    {
        BaseKey = GetBaseKey();
        Id = id.ToString();
        SessionSaver.AddContainer(this);
    }

    public SaveDataContainer(string id)
    {
        BaseKey = GetBaseKey();
        Id = id;
        SessionSaver.AddContainer(this);
    }

    private string GetBaseKey()
    {
        return GetType().ToString();
        //StackTrace stackTrace = new StackTrace();
        ////StackTrace.GetFrame(1) will return the constructor, GetFrame(2) will return the method creating this instance.
        ////Then ReflectedType will return the class that created this instance
        //return stackTrace.GetFrame(2).GetMethod().ReflectedType.ToString();
    }

    //public JSONObject GetJsonObject()
    //{
    //    var content = new JSONObject;
    //    foreach (var field in GetType().GetFields())
    //    {
    //        content[field.Name] = field.GetValue(this);
    //    }
    //}
}
