using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;
using UnityEngine.EventSystems;
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
			var httpPosition = gameObject.name.IndexOf("http");
			var url = gameObject.name.Substring(httpPosition, gameObject.name.Length-httpPosition);
			JavascriptMethodCaller.OpenURL(url);

			var button = GetComponent<Button>();
			if (button)
			{
				//Make sure out release is triggered if this is a button, because our release is probably being blocked by a new browser window)
				button.onClick.Invoke();
			}
		}
	}
}
