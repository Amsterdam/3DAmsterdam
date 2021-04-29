using Netherlands3D.Help;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Underground;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
{
    public class MaskPaintTool : MonoBehaviour
    {
        [SerializeField]
        private GridSelection gridSelection;
        private Bounds previousBounds;

        [SerializeField]
        private RuntimeMask runtimeRectangularMask;

        public void StartPaintSelection()
        {
            gridSelection.gameObject.SetActive(true);
            GridSelection.onGridSelected += SelectedGridBounds;
            runtimeRectangularMask.gameObject.SetActive(true);
            runtimeRectangularMask.Clear();
            if (previousBounds != default)
            {
                runtimeRectangularMask.MoveToBounds(previousBounds);
            }
        }

        private void SelectedGridBounds(Bounds bounds)
        {
            previousBounds = bounds;
            runtimeRectangularMask.MoveToBounds(bounds);
            PropertiesPanel.Instance.OpenLayers();
            gridSelection.gameObject.SetActive(false);
            GridSelection.onGridSelected -= SelectedGridBounds;
        }
    }
}