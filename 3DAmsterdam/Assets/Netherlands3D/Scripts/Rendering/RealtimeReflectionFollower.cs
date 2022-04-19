using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Rendering {
	public class RealtimeReflectionFollower : MonoBehaviour
	{
		//This makes this relatime reflectionprobe move with the camera, so we have vanilla Unity realtime reflections when it is enabled
		private void LateUpdate()
		{
			this.transform.position = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position;
		}
	}
}