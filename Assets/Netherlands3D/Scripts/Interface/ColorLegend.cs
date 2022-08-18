using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface {
    public class ColorLegend : MonoBehaviour
    {
        [SerializeField]
        private ColorPalette colorPalette;

        [SerializeField]
        private GameObject paletteColorPrefab;

        private void Awake()
        {
            GenerateColorsList();
        }

        private void GenerateColorsList()
        {
            foreach (var namedColor in colorPalette.colors)
            {
                var newNamedColor = Instantiate(paletteColorPrefab, this.transform);
                newNamedColor.name = namedColor.name;
                newNamedColor.GetComponentInChildren<Image>(true).color = namedColor.color;
                newNamedColor.GetComponentInChildren<Text>(true).text = namedColor.name;
            }
        }
    }
}