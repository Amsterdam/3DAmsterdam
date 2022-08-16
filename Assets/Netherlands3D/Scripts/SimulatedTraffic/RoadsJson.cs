using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Roads
{
    public string type;
    public string name;
    public Crs crs = new Crs();
    public RoadItem[] features;
}
[System.Serializable]
public class Crs
{
    public string type;
    public Properties properties = new Properties();
}
[System.Serializable]
public class Properties
{
    public string name;
}
[System.Serializable]
public class RoadItem
{
    public string type;
    public RoadProperties properties = new RoadProperties();
    public Geometry geometry = new Geometry();
}
[System.Serializable]
public class RoadProperties
{
    public string highway;
    public string oneway;
    public string name;
    public string maxspeed;
    public string smoothness;
    public string surface;
    public string lit;
    public string bicycle;
    public string surfacecolour;
    public string width;
    public string litindirect;
    public string cycleway;
    public string access;
    public string electrified;
    public string frequency;
    public string gauge;
    public string railway;
    public string voltage;
    public string moped;
    public string psv;
    public string onewaybicycle;
    public string lanes;
    public string _ref;
    public string foot;
    public string layer;
    public string bridge;
    public string traffic_sign;
    public string cyclewayright;
    public string cyclewayrightwidth;
    public string service;
    public string cyclewayrightsurface;
    public string cyclewayrightsurfacecolour;
    public string cyclewaysurface;
    public string cyclewaywidth;
    public string cyclewaysurfacecolour;
    public string turnlanes;
    public string mofa;
}
[System.Serializable]
public class Geometry
{
    public string type;
    public List<LongitudeLatitude> coordinates = new List<LongitudeLatitude>();
}

[System.Serializable]
public class LongitudeLatitude
{
    public double longitude;
    public double latitude;

    //Another Contructor
    public LongitudeLatitude(double[] inputcords)
    {
        longitude = inputcords[0];
        latitude = inputcords[1];
    }
    // Constructor
    public LongitudeLatitude()
    {
        
    }
  
}
