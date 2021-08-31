using Netherlands3D.Cameras;
using TMPro;
using UnityEngine;

namespace Netherlands3D.LayerSystem
{
	public class FaceToCamera : MonoBehaviour
	{
		private TextMeshPro textMeshPro;
		private MeshRenderer renderer;
		private float hideDistance = 0;
		public float HideDistance { get => hideDistance; set => hideDistance = value; }

		private void Start()
		{
			textMeshPro = GetComponent<TextMeshPro>();
		}

		void Update()
		{
			DisplayAreasBasedOnCameraTransform();
		}

		private void DisplayAreasBasedOnCameraTransform()
		{
			Transform cameraTransform = CameraModeChanger.Instance.ActiveCamera.transform;

			Vector3 cameraPosition = cameraTransform.position;
			Quaternion cameraRotation = cameraTransform.rotation;

			float camheight = cameraTransform.position.y;
			float camAngle = cameraTransform.localRotation.eulerAngles.x;
			float maxDistance = 0;

			if (camAngle > 45f) // looking down;
			{
				maxDistance = 2 * 2000;
			}
			else
			{
				maxDistance = 5 * camheight;
			}
			if (camheight < HideDistance)
			{
				maxDistance = 0;
			}

			if ((transform.position - cameraPosition).magnitude < maxDistance)
			{
				textMeshPro.enabled = true;
			}
			else
			{
				textMeshPro.enabled = false;
			}

			float characterSize = (1 + Mathf.Max(camheight / 500, 0)) * 200;

			transform.rotation = cameraRotation;
			textMeshPro.fontSize = characterSize;
		}
	}
}