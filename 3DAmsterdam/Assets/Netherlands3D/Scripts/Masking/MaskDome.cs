using Netherlands3D.Cameras;
using Netherlands3D.Events;
using Netherlands3D.Help;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Masking
{
    public class MaskDome : Interactable
    {
        [Header("Trigger events")]
        [SerializeField] private TriggerEvent abortMasking;
        private RuntimeMask runtimeMask;
        void Awake()
        {
            runtimeMask = GetComponent<RuntimeMask>();
            abortMasking.AddListenerStarted(Escape);
        }

		private void OnEnable()
		{
            HelpMessage.Show("Beweeg uw muis over het maaiveld om er doorheen te kijken.");
            TakeInteractionPriority();
            runtimeMask.ClearAllMasks();
        }
		public override void OnDisable()
		{
            StopInteraction();
        }

		public override void Escape()
		{
			base.Escape();
            gameObject.SetActive(false);
		}

        void Update()
        {
            MoveWithPointer();
            runtimeMask.ApplyNewPositionAndScale();
        }

        private void MoveWithPointer()
        {
            if (Selector.Instance.HoveringInterface())
            {
                transform.transform.localScale = Vector3.zero;
                return;
            }

            transform.position = CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld();
            transform.transform.localScale = Vector3.one * runtimeMask.MaskScaleMultiplier * CameraModeChanger.Instance.CurrentCameraControls.GetCameraHeight() * (((CameraModeChanger.Instance.ActiveCamera.orthographic) ? 1.0f : (CameraModeChanger.Instance.ActiveCamera.fieldOfView/60)));
        }
    }
}