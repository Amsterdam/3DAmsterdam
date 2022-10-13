using Netherlands3D.Core.Colors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColorAtStart : MonoBehaviour
{
    [SerializeField] private ColorPalette colorPalette;
    void Start()
    {
        var totalColorWeight = 0;
        foreach(var paletteColor in colorPalette.colors)
        {
            totalColorWeight += paletteColor.count;
        }

        int colorPercentage = Random.Range(0, totalColorWeight);
        this.GetComponent<Renderer>().material.color = GetColorByWeight(colorPercentage);
    }

    private Color GetColorByWeight(float colorPercentage)
    {
        Color32 defaultColor = Color.gray;
        foreach (var paletteColor in colorPalette.colors)
        {
            if (colorPercentage < paletteColor.count)
            {
                return paletteColor.color;
            }
            else
            {
                colorPercentage -= paletteColor.count;
            }
        }
        return defaultColor;
    }
}
