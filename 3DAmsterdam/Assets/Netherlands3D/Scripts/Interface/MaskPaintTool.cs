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
        [SerializeField] private TriggerEvent startCreatingMask;
        [SerializeField] private TriggerEvent abortSelection;
        [SerializeField] private ObjectEvent receivedBounds;

        [Header("Trigger events")]
        [SerializeField] private ObjectEvent changeGridSelectionColor;
        [SerializeField] private TriggerEvent requestGridSelection;
        [SerializeField] private TriggerEvent createdMask;

        [SerializeField]
        private RuntimeMask runtimeRectangularMask;

        private string helpMessage = "<b>Shift+Klik+Sleep</b> om het masker gebied te selecteren";

        [SerializeField]
        private Color selectionColor;


        private void Awake()
		{
            startCreatingMask.AddListenerStarted(StartPaintingMask);
            abortSelection.AddListenerStarted(Abort);
        }

		private void StartPaintingMask()
        {

            this.gameObject.SetActive(true);
            requestGridSelection.InvokeStarted();
            changeGridSelectionColor.InvokeStarted(selectionColor);
            receivedBounds.RemoveAllListenersStarted();
            receivedBounds.AddListenerStarted((bounds) => { SelectedMaskBounds((Bounds)bounds); });
        }

        private void Abort()
        {
            gameObject.SetActive(false);
        }

		private void SelectedMaskBounds(Bounds bounds)
        {
            runtimeRectangularMask.MoveToBounds(bounds);
            createdMask.InvokeStarted();
            PropertiesPanel.Instance.OpenLayers();
            gameObject.SetActive(false);
        }
        private void OnEnable()
        {
           HelpMessage.Show(helpMessage);
        }
    }
}