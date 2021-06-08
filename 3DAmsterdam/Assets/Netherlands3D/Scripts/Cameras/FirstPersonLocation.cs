﻿using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface.Menu;
using Netherlands3D.Interface.SidePanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	[RequireComponent(typeof(WorldPointFollower))]
	public class FirstPersonLocation : PlaceOnClick, IPointerClickHandler
	{
		private CameraModeChanger cameraModeChanger;

		[SerializeField]
		private Animator placedAnimator;

		[HideInInspector]
		public Quaternion savedRotation = Quaternion.Euler(Vector3.zero);
		public override void Start()
		{
			base.Start();

			var gridSelection = FindObjectOfType<GridSelection>();
			if(gridSelection)
				gridSelection.gameObject.SetActive(false);

			cameraModeChanger = CameraModeChanger.Instance;
			cameraModeChanger.OnGodViewModeEvent += EnableObject;
			cameraModeChanger.OnFirstPersonModeEvent += DisableObject;

			if (waitingForClick)
			{
				HelpMessage.Instance.Show("<b>Klik</b> op het maaiveld om een camerastandpunt te plaatsen\n\nGebruik de <b>Escape</b> toets om te annuleren");
				PropertiesPanel.Instance.OpenCustomObjects();
			}
		}

		protected override void Placed()
		{
			base.Placed();
			placedAnimator.enabled = true;
			HelpMessage.Instance.Show("<b>Klik</b> op het nieuwe camerastandpunt om rond te lopen");
		}

		private void EnableObject()
		{
			gameObject.SetActive(true);
		}

		private void DisableObject()
		{
			gameObject.SetActive(false);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (waitingForClick) return;

			cameraModeChanger.FirstPersonMode(WorldPointerFollower.WorldPosition, savedRotation);
			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{ 
			if(cameraModeChanger){
				cameraModeChanger.OnGodViewModeEvent -= EnableObject;
				cameraModeChanger.OnFirstPersonModeEvent -= DisableObject;
			}
		}
	}
}