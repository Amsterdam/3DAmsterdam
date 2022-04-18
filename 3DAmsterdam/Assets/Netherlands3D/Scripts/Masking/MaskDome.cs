using Netherlands3D.Cameras;
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
        private RuntimeMask runtimeMask;
        void Start()
        {
            runtimeMask = GetComponent<RuntimeMask>();
        }

		private void OnEnable()
		{
            ServiceLocator.GetService<HelpMessage>().Show("Beweeg uw muis over het maaiveld om er doorheen te kijken.");
            TakeInteractionPriority();
		}
		protected override void OnDisable()
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
            if (ServiceLocator.GetService<Selector>().HoveringInterface())
            {
                transform.transform.localScale = Vector3.zero;
                return;
            }

            transform.position = ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.GetPointerPositionInWorld();
            transform.transform.localScale = Vector3.one * runtimeMask.MaskScaleMultiplier * ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position.y;
        }
    }
}