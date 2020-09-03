using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;
using UnityEngine.EventSystems;

namespace Amsterdam3D.Interface
{
	public class OpenURL : ChangePointerStyleHandler, IPointerDownHandler
	{
		public void OnPointerDown(PointerEventData eventData)
		{
			OpenURLByGameObjectName();
		}

		private void OpenURLByGameObjectName()
		{
			JavascriptMethodCaller.OpenURL(gameObject.name);
		}
	}
}
