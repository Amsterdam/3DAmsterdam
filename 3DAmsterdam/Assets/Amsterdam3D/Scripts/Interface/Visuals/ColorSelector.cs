using System.Net;
using UnityEngine;

public abstract class ColorSelector : MonoBehaviour
{
	public delegate void SelectedNewColor(Color color, ColorSelector selector);
	public SelectedNewColor selectedNewColor;

	public abstract void ChangeColorInput(Color inputColor);
}