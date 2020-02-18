using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Attributes : MonoBehaviour

{
    public string id;
    [SerializeField]
    public List<AttributeData> items = new List<AttributeData>();
    
}

[System.Serializable]
public class AttributeData
{
    
    public string name;
    public string value;
    public AttributeData(string Name, string Value)
    {
        name = Name;
        value = Value;
    }
}


