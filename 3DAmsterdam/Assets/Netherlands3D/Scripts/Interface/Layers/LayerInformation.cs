using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Layers
{
    public class LayerInformation : MonoBehaviour
    {
        /// <summary>
        /// Make sure any other layer information panels are hidden.
        /// We use unique objects for the layer panels, because there is too much difference in hierarchy
        /// to be able to use a single object and swap data. (url's in unity texts are covered by buttons to make them clickable)
        /// </summary>
        void OnEnable()
        {
            var layerInformationPanels = FindObjectsOfType<LayerInformation>();
            foreach (var panel in layerInformationPanels)
            {
                if (panel != this) panel.gameObject.SetActive(false);
            }
        }
    }
}