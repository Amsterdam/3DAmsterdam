using Amsterdam3D.CameraMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class FirstPersonObject : PlaceOnClick, IPointerClickHandler
	{
		private CameraModeChanger cameraModeChanger;

		public bool placed = false;
		private WorldPointFollower follower;

		[HideInInspector]
		public Quaternion savedRotation = Quaternion.Euler(Vector3.zero);
		public override void Start()
		{
			base.Start();

			cameraModeChanger = CameraModeChanger.Instance;
			cameraModeChanger.OnGodViewModeEvent += EnableObject;
			cameraModeChanger.OnFirstPersonModeEvent += DisableObject;
			follower = GetComponent<WorldPointFollower>();
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

			cameraModeChanger.FirstPersonMode(follower.WorldPosition, savedRotation);
			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			cameraModeChanger.OnGodViewModeEvent -= EnableObject;
			cameraModeChanger.OnFirstPersonModeEvent -= DisableObject;
		}
	}
}