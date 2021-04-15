using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimitedNumericInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private float scrollWheelSensitivity = 1.0f;

	[SerializeField]
	private InputField inputField;

	public delegate void AddedOffset(int addedOffset);
	public AddedOffset addedOffset;

	private int offset = 0;

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		StartCoroutine(ReadScrollWheelInput());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
	}

	/// <summary>
	/// Change the input field text value, but only if we are not focussing this field and changing it manualy (and are not scrolling the input)
	/// </summary>
	/// <param name="textInput">The new text value</param>
	public void SetInputText(string textInput){
		if(!inputField.isFocused || ( inputField.isFocused && offset != 0 ))
			inputField.text = textInput;
	}

	/// <summary>
	/// Keeps checking the scroll input and invokes the addOffset event when needed
	/// </summary>
	IEnumerator ReadScrollWheelInput() {
		while (true){
			offset = Mathf.RoundToInt(Input.mouseScrollDelta.y * scrollWheelSensitivity);
			if(offset != 0.0f)
				addedOffset.Invoke(offset);
			yield return null;
		}	
	}
}
