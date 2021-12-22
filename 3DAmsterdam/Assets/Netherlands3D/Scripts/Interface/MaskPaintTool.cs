using Netherlands3D.Help;
using Netherlands3D.Interface.Layers;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Masking;
using UnityEngine;

namespace Netherlands3D.Interface.Tools
{
    public class MaskPaintTool : MonoBehaviour
    {
        [SerializeField]
        private GridSelection gridSelection;

        [SerializeField]
        private RuntimeMask runtimeRectangularMask;

        private string helpMessage = "<b>Shift+Klik+Sleep</b> om het masker gebied te selecteren";

        [SerializeField]
        private InterfaceLayer maskLayer;

        [SerializeField]
        private Material maskBlockMaterial;

        private void OnEnable()
        {
            HelpMessage.Instance.Show(helpMessage);

            gridSelection.onGridSelected.RemoveAllListeners();
            gridSelection.StartSelection(maskBlockMaterial);
            gridSelection.onGridSelected.AddListener(SelectedMaskBounds);
            gridSelection.onToolDisabled.AddListener(ToolWasDisabled);
            runtimeRectangularMask.Clear();
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
            PropertiesPanel.Instance.OpenLayers();

            maskLayer.ToggleLinkedObject(true);
            maskLayer.ExpandLayerOptions(true);
        }
    }
}