using Netherlands3D.Cameras;
using Netherlands3D.Events;
using Netherlands3D.Help;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	[RequireComponent(typeof(WorldPointFollower))]
	public class FirstpersonCameraPlacer : Interactable
	{
		[HideInInspector] public Quaternion cameraRotation = Quaternion.identity;
		[SerializeField] private TriggerEvent startPlacement;
		private WorldPointFollower worldPointFollower;

		private bool placing = false;

		private void Awake()
        {
			worldPointFollower = GetComponent<WorldPointFollower>();
			startPlacement.started.AddListener(StartPlacement);

			//add listener for click
        }

        private void StartPlacement()
        {
			gameObject.SetActive(true);
			HelpMessage.Show("<b>Klik</b> op het maaiveld om een camerastandpunt te plaatsen\n\nGebruik de <b>Escape</b> toets om te annuleren");
			PropertiesPanel.Instance.OpenCustomObjects();
			placing = true;
			StartCoroutine(FollowPointer());
		}

        public override void OnDisable()
        {
			base.OnDisable();

			StopAllCoroutines();
			placing = false;

		}

        private IEnumerator FollowPointer()
        {
			yield return new WaitForEndOfFrame();

			while (placing)
			{
				yield return new WaitForEndOfFrame();
				var targetLocation = (Selector.hits.Length > 0) ? Selector.hits[0].point : CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld();
				worldPointFollower.AlignWithWorldPosition(targetLocation + Vector3.up * 1.8f);
			}
		}

        private void Placed()
		{
			cameraRotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
			CameraModeChanger.Instance.FirstPersonMode(worldPointFollower.WorldPosition, cameraRotation);
			StopInteraction();

			gameObject.SetActive(false);
		}
	}
}