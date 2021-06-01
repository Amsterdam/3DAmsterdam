using Netherlands3D.Help;
using Netherlands3D.Interface.Layers;
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

        [SerializeField]
        private RectTransform maskSettings;
        [SerializeField]
        private InterfaceLayer maskLayer;

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

            if (!maskSettings.gameObject.activeSelf)
                maskLayer.ToggleLayerOpenedWithCustomOptions(maskSettings.gameObject);

            gridSelection.gameObject.SetActive(false);
        }
    }
}