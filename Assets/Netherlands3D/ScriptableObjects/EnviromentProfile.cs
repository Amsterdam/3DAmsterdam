using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EnviromentProfile", order = 0)]
[System.Serializable]
public class EnviromentProfile : ScriptableObject
{
    public string enviromentName = "Clear";

    public Color fogColorDay = Color.white;
    public Color fogColorNight = Color.black;

    //3 Color gradients
    public Color[] skyColorsDay = new Color[3];
    public Color[] skyColorsNight = new Color[3];

    [Header("Textured sky settings")]
    public Cubemap skyMap;
    public float exposureDay = 1.0f;
    public float exposureNight = 0.1f;

    public Color skyTintColorDay = Color.gray;
    public Color skyTintColorNight = Color.blue;

    public Texture2D sunTexture; 
    public Texture2D haloTexture; 
    public Color sunTextureTintColor = Color.yellow;
    public Color sunHaloTextureTintColor = Color.yellow;
}
