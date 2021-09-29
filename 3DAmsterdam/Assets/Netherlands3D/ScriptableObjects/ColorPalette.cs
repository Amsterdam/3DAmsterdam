using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Netherlands3D
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorPalette", order = 1)]
    public class ColorPalette : ScriptableObject
    {
        public List<NamedColor> colors;

        public Color this[string colorName]
        {
            get { return colors.First(namedColor => namedColor.name == colorName).color; }
        }
    }

    [System.Serializable]
    public class NamedColor
    {
        public string name = "";
        public Color color;
    }
}