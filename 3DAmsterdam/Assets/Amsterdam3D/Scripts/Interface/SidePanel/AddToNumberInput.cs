using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AddToNumberInput : MonoBehaviour, IScrollHandler
{
	[SerializeField]
	private InputField inputField;

	[SerializeField]
	private float addWithScroll = 1.0f;

	public void AddValue(float amount)
    {
		if (double.TryParse(inputField.text, out double inputValue))
		{
			inputValue += amount;
			inputField.text = inputValue.ToString(CultureInfo.InvariantCulture);

			//Force a value change event
			inputField.onValueChanged.Invoke(null);
		}
    }

	public void OnScroll(PointerEventData eventData)
	{
		if (eventData.scrollDelta.y > 0)
		{
			AddValue(addWithScroll);
		}
		else if (eventData.scrollDelta.y < 0)
		{
			AddValue(-addWithScroll);
		}
	}
}
