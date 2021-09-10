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
	public class FlipToCamera : MonoBehaviour
	{
		private float currentAngle;
		public List<string> UniqueNamesList { get; internal set; }

		void Start()
		{
			currentAngle = transform.transform.localRotation.eulerAngles.y;
		}

		private void OnDestroy()
		{
			UniqueNamesList.Remove(name);
		}

		void Update()
		{
			FlipTextByLookDirection();
		}

		private void FlipTextByLookDirection()
		{
			float cameraAngle = CameraModeChanger.Instance.ActiveCamera.transform.rotation.eulerAngles.y;
			float angleDelta;
			angleDelta = Mathf.DeltaAngle(cameraAngle, currentAngle);

			//Flip so the text is not upside down in the camera view
			if (Mathf.Abs(angleDelta) > 90)
			{
				transform.Rotate(Vector3.forward, 180f, Space.Self);
				currentAngle = transform.transform.localRotation.eulerAngles.y;
			}

			//Turn down if camera is below the text
			this.transform.localEulerAngles = new Vector3(
				(CameraModeChanger.Instance.ActiveCamera.transform.position.y < this.transform.position.y) ? -90 : 90,
				this.transform.localEulerAngles.y,
				this.transform.localEulerAngles.z
			);
		}
	}
}