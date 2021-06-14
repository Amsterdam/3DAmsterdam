using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorPalette", order = 1)]
    public class ColorPalette : ScriptableObject
    {
        public List<NamedColor> colors;
    }

    [System.Serializable]
    public class NamedColor
    {
        public string name = "";
        public Color color;
    }
}