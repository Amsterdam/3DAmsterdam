using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Cameras
{
	public class PointerLock
	{
		public enum Mode
		{
			DEFAULT,
			FIRST_PERSON
		}

		private static Mode mode = Mode.DEFAULT;

		public static void SetMode(Mode newMode){
			mode = newMode;
			switch (mode)
			{
				case Mode.DEFAULT:
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					break;
				case Mode.FIRST_PERSON:
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
					break;
			}
		}

		public static Mode GetMode()
		{
			return mode;
		}
	}
}
