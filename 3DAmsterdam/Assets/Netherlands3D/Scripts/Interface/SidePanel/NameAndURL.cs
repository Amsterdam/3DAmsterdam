
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
	public class NameAndURL : OpenURL
	{
		[SerializeField]
		private Text titleText;

		void Awake()
		{
			titleText.GetComponent<Text>();
		}

		public void SetURL(string urlName, string urlPath)
		{
			titleText.text = urlName;
			gameObject.name = urlPath;
		}
	}
}