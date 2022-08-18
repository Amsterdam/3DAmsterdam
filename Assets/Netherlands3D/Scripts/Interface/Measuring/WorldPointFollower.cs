using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	public class WorldPointFollower : MonoBehaviour
	{
		protected RectTransform rectTransform;

		private Vector3 worldPosition = Vector3.zero;
		public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }

		public bool isEnabled = true;
		//private bool insideView = false;

		private float maxRenderDistanceAtGround = 100;

		private Graphic[] graphics;

		public virtual void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			graphics = GetComponentsInChildren<Graphic>(true);
		}

		public virtual void AlignWithWorldPosition(Vector3 newWorldPosition)
		{
			WorldPosition = newWorldPosition;
		}

		protected virtual void Update()
		{
			AutoHideByCamera();
		}

		private void DrawGraphics(bool draw = true)
		{
            if (isEnabled)
            {
				foreach (var graphic in graphics)
					graphic.enabled = draw;
			}
			else
            {
				foreach (var graphic in graphics)
					graphic.enabled = false;
			}
			
		}

		/// <summary>
		/// Hides the annotation based on camera type and distance
		/// </summary>
		public void AutoHideByCamera()
		{
			//Always hide outside viewport
			var screenPosition = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.WorldToScreenPoint(WorldPosition);
			if (screenPosition.x > Screen.width || screenPosition.x < 0 || screenPosition.y > Screen.height || screenPosition.y < 0 || screenPosition.z < 0)
			{
				DrawGraphics(false);
				return;
			}

			//Else maybe hide depending on distance from streetview camera
			if (ServiceLocator.GetService<CameraModeChanger>().CurrentMode == CameraMode.StreetView)
			{
				var distance = WorldPosition - ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position;
				if (distance.magnitude > maxRenderDistanceAtGround)
				{
					DrawGraphics(false);
					return;
				}
			}

			DrawGraphics(true);
			rectTransform.position = screenPosition;
		}

	}
}