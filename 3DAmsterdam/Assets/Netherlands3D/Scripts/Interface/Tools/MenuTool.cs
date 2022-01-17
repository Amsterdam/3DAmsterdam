using Netherlands3D.Interface.Tools;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Tools
{
	public class MenuTool : MonoBehaviour
	{
		[SerializeField]
		private GameObject toolGameObject;

		private void Awake()
		{
			if (toolGameObject)
				toolGameObject.AddComponent<Tool>().SetMenuTool(this);
		}

		public void ActivateToolGameObject()
		{
			toolGameObject.SetActive(false); //Force enable/disable
			toolGameObject.SetActive(true);
		}
	}
}