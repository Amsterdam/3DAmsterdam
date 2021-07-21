using Netherlands3D.Interface.Tools;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Tools
{
	public class MenuTool : MonoBehaviour
	{
		[SerializeField]
		private GameObject toolGameObject;

		private void Awake()
		{
			if (toolGameObject)
				toolGameObject.AddComponent<ToolMenuLink>().SetMenuTool(this);
		}

		public void ActivateToolGameObject()
		{
			toolGameObject.SetActive(true);
		}
	}
}