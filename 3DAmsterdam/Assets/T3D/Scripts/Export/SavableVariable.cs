using System;
using ConvertCoordinates;
using UnityEngine;

public abstract class SaveableVariable<T>
{
    public string Key { get; set; }
    public T Value { get; protected set; }
    public abstract void Save();
    public abstract void Load();
    public abstract void Delete();

    public SaveableVariable(string key)
    {
        Key = key;

        if (SessionSaver.HasLoaded)
            Load();

        SessionSaver.Loader.LoadingCompleted += Loader_LoadingCompleted;

        Save();
    }

    //public SaveableVariable(string key, T defaultValue)
    //{
    //    Key = key;

    //    if (SessionSaver.HasLoaded)
    //        Load();

    //    SessionSaver.Loader.LoadingCompleted += Loader_LoadingCompleted;
    //    SetValue(defaultValue);
    //    //Save();
    //}

    ~SaveableVariable()
    {
        SessionSaver.Loader.LoadingCompleted -= Loader_LoadingCompleted;
    }

    private void Loader_LoadingCompleted(bool loadSucceeded)
    {
        if (loadSucceeded)
        {
            Load();
            Save();
        }

        //SessionSaver.Loader.LoadingCompleted -= Loader_LoadingCompleted;
    }

    public void SetValue(T value)
    {
        Value = value;
        Save();
    }
}

public class SaveableFloat : SaveableVariable<float>
{
    public SaveableFloat(string key) : base(key)
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

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key);
    }
}

public class SaveableInt : SaveableVariable<int>
{
    public SaveableInt(string key) : base(key)
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

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key);
    }
}

public class SaveableString : SaveableVariable<string>
{
    public SaveableString(string key) : base(key)
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

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key);
    }
}

public class SaveableVector3 : SaveableVariable<Vector3>
{
    public SaveableVector3(string key) : base(key)
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

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key + ".x");
        SessionSaver.DeleteKey(Key + ".y");
        SessionSaver.DeleteKey(Key + ".z");
    }
}

public class SaveableVector3RD : SaveableVariable<Vector3RD>
{
    public SaveableVector3RD(string key) : base(key)
    {
    }

    public override void Load()
    {
        var x = SessionSaver.LoadFloat(Key + ".x");
        var y = SessionSaver.LoadFloat(Key + ".y");
        var z = SessionSaver.LoadFloat(Key + ".z");

        Value = new Vector3RD(x, y, z);
    }

    public override void Save()
    {
        SessionSaver.SaveFloat(Key + ".x", (float)Value.x);
        SessionSaver.SaveFloat(Key + ".y", (float)Value.y);
        SessionSaver.SaveFloat(Key + ".z", (float)Value.z);
    }

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key + ".x");
        SessionSaver.DeleteKey(Key + ".y");
        SessionSaver.DeleteKey(Key + ".z");
    }
}

public class SaveableBool : SaveableVariable<bool>
{
    public SaveableBool(string key) : base(key)
    {
    }

    public override void Load()
    {
        Value = SessionSaver.LoadBool(Key);
    }

    public override void Save()
    {
        SessionSaver.SaveBool(Key, Value);
    }

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key);
    }
}

//public class SaveableIntArray : SaveableVariable<int[]>
//{
//    public SaveableIntArray(string key) : base(key)
//    {
//    }

//    public override void Load()
//    {
//        var length = SessionSaver.LoadInt(Key + ".Length");
//        Value = new int[length];

//        for (int i = 0; i < Value.Length; i++)
//        {
//            SessionSaver.LoadInt(Key + i);
//        }
//    }

//    public override void Save()
//    {
//        SessionSaver.SaveInt(Key + ".Length", Value.Length);
//        for (int i = 0; i < Value.Length; i++)
//        {
//            SessionSaver.SaveInt(Key + "." + i, Value[i]);
//        }
//    }
//}

public class SaveableQuaternion : SaveableVariable<Quaternion>
{
    public SaveableQuaternion(string key) : base(key)
    {
    }

    public override void Load()
    {
        var x = SessionSaver.LoadFloat(Key + ".x");
        var y = SessionSaver.LoadFloat(Key + ".y");
        var z = SessionSaver.LoadFloat(Key + ".z");
        var w = SessionSaver.LoadFloat(Key + ".w");

        Value = new Quaternion(x, y, z, w);
    }

    public override void Save()
    {
        SessionSaver.SaveFloat(Key + ".x", Value.x);
        SessionSaver.SaveFloat(Key + ".y", Value.y);
        SessionSaver.SaveFloat(Key + ".z", Value.z);
        SessionSaver.SaveFloat(Key + ".w", Value.w);
    }

    public override void Delete()
    {
        SessionSaver.DeleteKey(Key + ".x");
        SessionSaver.DeleteKey(Key + ".y");
        SessionSaver.DeleteKey(Key + ".z");
        SessionSaver.DeleteKey(Key + ".w");
    }
}