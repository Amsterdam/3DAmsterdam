using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
	public void OpenURLByGameObjectName()
	{
		Application.OpenURL(gameObject.name);
	}
}
