using Netherlands3D.Events;
using Netherlands3D.Help;
using Netherlands3D.Interface.Layers;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Masking;
using System;
using UnityEngine;

namespace Netherlands3D.Interface.Tools
{
    public class MaskPaintTool : MonoBehaviour
    {   
        [Header("Listen to events")]
        [SerializeField]
        private TriggerEvent startCreatingMask;

        [Header("Trigger events")]
        [SerializeField]
        private TriggerEvent createdMask;

        [SerializeField]
        private GridSelection gridSelection;

        [SerializeField]
        private RuntimeMask runtimeRectangularMask;

        private string helpMessage = "<b>Shift+Klik+Sleep</b> om het masker gebied te selecteren";

        [SerializeField]
        private Material maskBlockMaterial;

        private void StartPaintingMask()
        {
            this.gameObject.SetActive(true);
        }

		private void Awake()
		{
            if (startCreatingMask) startCreatingMask.started.AddListener(StartPaintingMask);
        }

		private void OnEnable()
        {
            if(HelpMessage.Instance) HelpMessage.Instance.Show(helpMessage);

            gridSelection.onGridSelected.RemoveAllListeners();
            gridSelection.StartSelection(maskBlockMaterial);
            gridSelection.onGridSelected.AddListener(SelectedMaskBounds);
            gridSelection.onToolDisabled.AddListener(ToolWasDisabled);
        }

		private void ToolWasDisabled()
        {
            this.gameObject.SetActive(false);
		}

		private void OnDisable()
		{
            gridSelection.onGridSelected.RemoveListener(SelectedMaskBounds);
            gridSelection.onToolDisabled.RemoveListener(ToolWasDisabled);
        }

		private void SelectedMaskBounds(Bounds bounds)
        {
            runtimeRectangularMask.MoveToBounds(bounds);
            createdMask.started.Invoke();
            PropertiesPanel.Instance.OpenLayers();

            gridSelection.gameObject.SetActive(false);
        }
    }
}