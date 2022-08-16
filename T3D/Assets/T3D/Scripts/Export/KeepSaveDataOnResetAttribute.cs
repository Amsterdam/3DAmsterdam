using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class KeepSaveDataOnResetAttribute : Attribute
{
    public static string[] GetPersistentTypeKeys()
    {
        List<Type> readOnlyContainers = new List<Type>();
        var persistentContainers = from t in Assembly.GetExecutingAssembly().GetTypes()
                                   where t.IsDefined(typeof(KeepSaveDataOnResetAttribute))
                                   select t;

        int length = persistentContainers.Count();
        string[] keys = new string[length];
        var i = 0;
        foreach (var type in persistentContainers)
        {
            keys[i] = type.ToString();
            i++;
        }
        return keys;
    }
}
