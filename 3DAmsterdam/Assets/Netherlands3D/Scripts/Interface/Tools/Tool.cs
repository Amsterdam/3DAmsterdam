using Netherlands3D.Help;
using UnityEngine;

namespace Netherlands3D.Interface.Tools
{
	public class Tool : MonoBehaviour
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
			ToolBar.Instance.ActivatedTool(this);
		}

		private void OnDisable()
		{
			HelpMessage.Hide(true);
			ToolBar.Instance.DisabledTool(this);
		}
	}
}