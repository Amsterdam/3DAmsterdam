using System.Net;
using UnityEngine;

namespace Netherlands3D.Interface.Coloring
{
	public abstract class ColorSelector : MonoBehaviour
	{
		public delegate void SelectedNewColor(Color color, ColorSelector selector);
		public SelectedNewColor selectedNewColor;

		private void Awake()
		{
			selectedNewColor += PickedColorMessage;
		}

		private void PickedColorMessage(Color color, ColorSelector selector)
		{
		}

		public abstract void ChangeColorInput(Color inputColor);
	}
}