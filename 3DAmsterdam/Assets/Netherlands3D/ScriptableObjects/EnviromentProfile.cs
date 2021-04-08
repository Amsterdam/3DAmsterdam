using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EnviromentProfile", order = 0)]
[System.Serializable]
public class EnviromentProfile : ScriptableObject
{
    public string enviromentName = "Clear";

    public Texture2D skyIcon;

    public Cubemap skyMap;

    public float exposureDay = 1.0f;
    public float exposureNight = 0.1f;

    public Color fogColorDay = Color.white;
    public Color fogColorNight = Color.black;

    public Color ambientColorDay = Color.white;
    public Color ambientColorNight = Color.black;

    public Color skyTintColorDay = Color.gray;
    public Color skyTintColorNight = Color.blue;

    public Texture2D sunTexture; //Cloudy sky would require a more fuzzy sun visual
    public Color sunTextureTintColor = Color.yellow;
}
