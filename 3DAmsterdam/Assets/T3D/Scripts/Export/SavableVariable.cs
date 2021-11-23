using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SavableVariable<T>
{
    //public KeyValuePair<string, T> KeyValuePair { get; private set; }

    //public string Key { get; private set; }
    //public abstract KeyValuePair<string, T> Pair { get; protected set; }
    public string Key { get; set; }
    public T Value { get; protected set; }
    public abstract void Save();
    public abstract void Load();

    public SavableVariable(string key, bool loadSavedData)
    {
        Key = key;
        if (loadSavedData)
            Load();
    }

    public void SetValue(T value)
    {
        Value = value;
        Save();
    }
}

public class SaveableFloat : SavableVariable<float>
{
    public SaveableFloat(string key, bool loadSavedData) : base(key, loadSavedData)
    {
    }

    public override void Load()
    {
        Value = SessionSaver.LoadFloat(Key);
    }

    public override void Save()
    {
        SessionSaver.SaveFloat(Key, Value);
    }
}

public class SaveableInt : SavableVariable<int>
{
    public SaveableInt(string key, bool loadSavedData) : base(key, loadSavedData)
    {
    }

    public override void Load()
    {
        Value = SessionSaver.LoadInt(Key);
    }

    public override void Save()
    {
        SessionSaver.SaveInt(Key, Value);
    }
}

public class SaveableString : SavableVariable<string>
{
    public SaveableString(string key, bool loadSavedData) : base(key, loadSavedData)
    {
    }

    public override void Load()
    {
        Value = SessionSaver.LoadString(Key);
    }

    public override void Save()
    {
        SessionSaver.SaveString(Key, Value);
    }
}

public class SaveableVector3 : SavableVariable<Vector3>
{
    public SaveableVector3(string key, bool loadSavedData) : base(key, loadSavedData)
    {
    }

    public override void Load()
    {
        var x = SessionSaver.LoadFloat(Key + ".x");
        var y = SessionSaver.LoadFloat(Key + ".y");
        var z = SessionSaver.LoadFloat(Key + ".z");

        Value = new Vector3(x, y, z);
    }

    public override void Save()
    {
        SessionSaver.SaveFloat(Key + ".x", Value.x);
        SessionSaver.SaveFloat(Key + ".y", Value.y);
        SessionSaver.SaveFloat(Key + ".z", Value.z);
    }
}
