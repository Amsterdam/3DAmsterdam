using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KeyValueStore
{
    class SaveableAttribute : Attribute
    {
        public static Type[] GetTypesWithSaveableAttribute()
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.GetCustomAttributes<SaveableAttribute>().Count() > 0
                        select t;

            return types.ToArray();
        }
    }

    //public static class SaveableAttributeExtractor
    //{
    //    public static Type[] GetTypesWithSaveableAttribute()
    //    {
    //        var types = from t in Assembly.GetExecutingAssembly().GetTypes()
    //                    where t.GetCustomAttributes<SaveableAttribute>().Count() > 0
    //                    select t;

    //        return types.ToArray();
    //    }
    //}
}
