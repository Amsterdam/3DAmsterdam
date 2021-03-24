using Netherlands3D.Cameras;
using Netherlands3D.Interface.Menu;
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

		[HideInInspector]
		public Quaternion savedRotation = Quaternion.Euler(Vector3.zero);
		public override void Start()
		{
			base.Start();
			SwitchTool.Instance.ResetToDefault();
			cameraModeChanger = CameraModeChanger.Instance;
			cameraModeChanger.OnGodViewModeEvent += EnableObject;
			cameraModeChanger.OnFirstPersonModeEvent += DisableObject;
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