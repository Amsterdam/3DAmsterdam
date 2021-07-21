using UnityEngine;

namespace Netherlands3D.Interface.Tools
{
	public class ToolInteractable : MonoBehaviour
	{
		[SerializeField]
		private MenuTool menuTool;

		public MenuTool MenuTool { get => menuTool; private set => menuTool = value; }

		public void SetMenuTool(MenuTool tool)
		{
			MenuTool = tool;
		}

		private void OnEnable()
		{
			ToolBar.Instance.DisableOtherTools(this);
		}

		private void OnDisable()
		{
			ToolBar.Instance.DisableOtherTools();
		}
	}
}