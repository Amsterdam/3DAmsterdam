using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, IUniqueService> services = new Dictionary<Type, IUniqueService>();

    static ServiceLocator()
    {
        InstallServices();
    }

    public static void InstallServices()
    {
        var servicesInScene = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<IUniqueService>();
        foreach (var service in servicesInScene)
        {
            RegisterService(service);
        }
    }

    private static void RegisterService<T>(T service) where T : IUniqueService
    {        
        var type = service.GetType();
        Assert.IsFalse(services.ContainsKey(type), $"Service {type} already registered");

        services.Add(type, service);
        Debug.Log("Service installed: " + type);
    }

    private static void UnregisterService<T>(T service) where T : IUniqueService
    {
        var type = typeof(T);
        Assert.IsTrue(services.ContainsKey(type), $"Service {type} not registered");

        services.Remove(service.GetType());
        Debug.Log("Service uninstalled: " + type);
    }

    public static T GetService<T>() where T : IUniqueService
    {
        var type = typeof(T);
        if (!services.TryGetValue(type, out var service))
        {
            throw new Exception($"Service {type} not found");
        }
        return (T)service;
    }
}
