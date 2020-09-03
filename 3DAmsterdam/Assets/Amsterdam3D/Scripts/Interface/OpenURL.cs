using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class OpenURL : ChangePointerStyleHandler, IPointerDownHandler
	{
		public void OnPointerDown(PointerEventData eventData)
		{
			/*
			 * Release any UI items, so they will not get stuck to 
			 * the mouse when we return to the tab with our Unity canvas. (needs a click to release)
			*/
			EventSystem.current.SetSelectedGameObject(null);

			OpenURLByGameObjectName();
		}

		private void OpenURLByGameObjectName()
		{
			JavascriptMethodCaller.OpenURL(gameObject.name);
		}
	}
}
