using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;

namespace Amsterdam3D.Interface
{
	public class OpenURL : ChangePointerStyleHandler
	{
		public void OpenURLByGameObjectName()
		{
			JavascriptMethodCaller.OpenURL(gameObject.name);
		}
	}
}
