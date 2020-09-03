using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;

public class OpenURL : ChangePointerStyleHandler
{
	public void OpenURLByGameObjectName()
	{
		Application.OpenURL(gameObject.name);
	}
}
