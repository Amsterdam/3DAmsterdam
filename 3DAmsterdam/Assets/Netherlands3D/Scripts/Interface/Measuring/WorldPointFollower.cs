using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	public class WorldPointFollower : MonoBehaviour
	{
		private RectTransform rectTransform;

		private Vector3 worldPosition = Vector3.zero;
		public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }

		public bool isEnabled = true;
		private bool insideView = false;

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

		private void Update()
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
			var viewportPosition = CameraModeChanger.Instance.ActiveCamera.WorldToViewportPoint(WorldPosition);
			if (viewportPosition.x > 1 || viewportPosition.x < -1 || viewportPosition.y > 1 || viewportPosition.y < -1 || viewportPosition.z < 0)
			{
				DrawGraphics(false);
				return;
			}

			//Else maybe hide depending on distance from streetview camera
			if (CameraModeChanger.Instance.CameraMode == CameraMode.StreetView)
			{
				var distance = WorldPosition - CameraModeChanger.Instance.ActiveCamera.transform.position;
				if (distance.magnitude > maxRenderDistanceAtGround)
				{
					DrawGraphics(false);
					return;
				}
			}
			
			DrawGraphics(true);
			rectTransform.anchorMin = viewportPosition;
			rectTransform.anchorMax = viewportPosition;
		}

	}
}