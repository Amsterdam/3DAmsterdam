using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

using System.Diagnostics;
using System.Collections.Generic;

namespace KeyValueStore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class SaveableAttribute : Attribute
    {
        public int InstanceID { get; set; }

        public SaveableAttribute(int id = 0)
        {
            
        }

        public static Type[] GetTypesWithSaveableAttribute()
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsDefined(typeof(SaveableAttribute))
                        select t;

            return types.ToArray();
        }

        public static Dictionary<Type, PropertyInfo[]> GetPropertiesWithSaveableAttribute()
        {
            //UnityEngine.Debug.Log("getting properies" + test.num);
            var sw = new Stopwatch();
            sw.Start();

            var dict = new Dictionary<Type, PropertyInfo[]>();

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var t in types)
            {
                var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(prop => IsDefined(prop, typeof(SaveableAttribute)));

                if (properties.Count() > 0)
                {
                    dict.Add(t, properties.ToArray());
                }
            }

            sw.Stop();

            UnityEngine.Debug.Log(string.Format("Elapsed={0}", sw.Elapsed.TotalMilliseconds));
            return dict;
        }

        public static void Test()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(SaveableAttribute), false);
            print("count: " + attributes.Length);
            foreach (var attribute in attributes)
            {
                print(attribute);
            }
        }

        public static void print(object msg)
        {
            UnityEngine.Debug.Log(msg);
        }
    }
}
