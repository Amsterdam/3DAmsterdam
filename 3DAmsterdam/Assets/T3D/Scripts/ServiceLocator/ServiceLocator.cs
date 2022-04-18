using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using Netherlands3D.Cameras;

public static class ServiceLocator
{
    private static Dictionary<Type, IUniqueService> services = new Dictionary<Type, IUniqueService>();
    public static Dictionary<Type, IUniqueService> Test => services;

    static ServiceLocator()
    {
        //services = new Dictionary<Type, IUniqueService>();
        InstallServices();
    }

    public static void InstallServices()
    {
        services = new Dictionary<Type, IUniqueService>();
        var servicesInScene = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<IUniqueService>();
        foreach (var service in servicesInScene)
        {
            RegisterService(service);
        }
    }

    //private static void RegisterService(Type type, IUniqueService service)
    //{
    //    Assert.IsFalse(services.ContainsKey(type), $"Service {type} already registered");

    //    services.Add(type, service);
    //    Debug.Log("Service installed: " + type);
    //}

    private static void RegisterService<T>(T service) where T : IUniqueService
    {        
        var type = service.GetType();
        Assert.IsFalse(services.ContainsKey(type), $"Service {type} already registered");

        services.Add(type, service);
    }

    private static void UnregisterService<T>(T service) where T : IUniqueService
    {
        var type = typeof(T);
        Assert.IsTrue(services.ContainsKey(type), $"Service {type} not registered");

        services.Remove(service.GetType());
    }

    public static T GetService<T>() where T : IUniqueService
    {
        if (services == null)
            InstallServices();

        var type = typeof(T);
        if (!services.TryGetValue(type, out var service))
        {
            throw new Exception($"Service {type} not found");
        }
        return (T)service;
    }
}
