using Netherlands3D.Core.Colors;
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

        [SerializeField]
        private Text titleText;

        private void Start()
        {
            GenerateColorsList();
        }

		private void OnValidate()
		{
            if(Application.isPlaying)
                GenerateColorsList();
        }

		private void GenerateColorsList()
        {
            foreach(Transform child in transform)
            {
                if (child != titleText.gameObject.transform) 
                {
                    Destroy(child.gameObject);
                }
			}
            if (!colorPalette) return;

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