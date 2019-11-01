using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstWeertype : MonoBehaviour
{
    public void Zon()
    {  
        EnviroSkyMgr.instance.ChangeWeather(0);
    }

    public void Bewolkt1()
    {
        EnviroSkyMgr.instance.ChangeWeather(1);
    }
    public void Bewolkt2()
    {
        EnviroSkyMgr.instance.ChangeWeather(3);
    }
    public void Bewolkt3()
    {
        EnviroSkyMgr.instance.ChangeWeather(4);
    }
    public void Regen1()
    {
        EnviroSkyMgr.instance.ChangeWeather(6);
    }
    public void Regen2()
    {
        EnviroSkyMgr.instance.ChangeWeather(7);
    }
    public void Mist()
    {
        EnviroSkyMgr.instance.ChangeWeather(5);
    }
}

