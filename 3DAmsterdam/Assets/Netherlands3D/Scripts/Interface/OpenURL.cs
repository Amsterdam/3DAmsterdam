using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.JavascriptConnection;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
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
			var httpPosition = gameObject.name.IndexOf("http");
			var url = gameObject.name.Substring(httpPosition, gameObject.name.Length-httpPosition);
			JavascriptMethodCaller.OpenURL(url);

			//Make sure to release everything manually (release event is blocked by our new browser window)
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
