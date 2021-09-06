/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Cameras;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Netherlands3D.LayerSystem
{
	public class FaceToCamera : MonoBehaviour
	{
		private TextMeshPro textMeshPro;

		private float hideDistance = 0;
		public float HideDistance { get => hideDistance; set => hideDistance = value; }
		public List<string> UniqueNamesList { get; internal set; }

		private void Start()
		{
			textMeshPro = GetComponent<TextMeshPro>();
		}

		private void OnDestroy()
		{
			UniqueNamesList.Remove(name);
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