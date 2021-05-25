﻿using Netherlands3D.Help;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Masking;
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

        private string helpMessage = "<b>Shift+Klik+Sleep</b> om het masker gebied te selecteren";

        public void WaitForMaskBounds()
        {
            HelpMessage.Instance.Show(helpMessage);
            gridSelection.onGridSelected.AddListener(SelectedMaskBounds);
            runtimeRectangularMask.gameObject.SetActive(true);
            runtimeRectangularMask.Clear();
        }

        private void SelectedMaskBounds(Bounds bounds)
        {
            previousBounds = bounds;
            runtimeRectangularMask.MoveToBounds(bounds);
            PropertiesPanel.Instance.OpenLayers();
            gridSelection.gameObject.SetActive(false);
        }
    }
}