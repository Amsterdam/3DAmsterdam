using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface
{
	public class SelectableText : MonoBehaviour, IPointerEnterHandler
	{
		[SerializeField]
		private TMP_InputField inputFieldSource;
		private TMP_InputField inputField;

		private TextMeshProUGUI text;
		private TextMeshProUGUI inputFieldText;

		[SerializeField]
		private bool readOnly = true;

		[System.Serializable]
		public class ChangedTextEvent : UnityEvent<string>{}
		[SerializeField]
		private ChangedTextEvent changedTextEvent;

		void Start()
		{
			text = this.GetComponent<TextMeshProUGUI>();
			text.raycastTarget = true;
		}

		public void SetFieldPrefab(TMP_InputField inputFieldPrefab)
		{
			inputFieldSource = inputFieldPrefab;
			//Make sure our mouse pointer turns into a text pointer when hovering, letting the user know its selectable
			gameObject.AddComponent<ChangePointerStyleHandler>().StyleOnHover = ChangePointerStyleHandler.Style.TEXT;
		}

		/// <summary>
		/// Clear the generated inputfield and its listeners
		/// </summary>
		public void Clear()
		{
			if (inputField)
			{
				Destroy(inputField);
				inputField.onEndEdit.RemoveAllListeners();
			}
		}

		private void OnEndEdit(string newString = "")
		{
			inputField.gameObject.SetActive(false);
			changedTextEvent?.Invoke(newString);
		}
		private void OnEdit(string newString = "")
		{
			changedTextEvent?.Invoke(newString);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (inputField)
			{
				DisplayInputWithText();
			}
			else if (text.text.Length > 0)
			{
				CreateInputFieldCopy();
			}
		}

		private void DisplayInputWithText()
		{
			inputField.gameObject.SetActive(true);

			//Copy the text from the source Text component as a value
			inputField.text = text.text;
		}

		private void CreateInputFieldCopy()
		{
			inputField = Instantiate(inputFieldSource, this.transform);
			inputFieldText = inputField.GetComponentInChildren<TextMeshProUGUI>();
			if(!inputFieldText) inputFieldText = inputField.GetComponent<TextMeshProUGUI>();
			//Match visuals of textfield
			inputFieldText.font = text.font;
			inputFieldText.fontSize = text.fontSize;
			inputFieldText.fontStyle = text.fontStyle;
			inputField.readOnly = readOnly;

			DisplayInputWithText();

			inputField.onEndEdit.AddListener(OnEndEdit);
			inputField.onValueChanged.AddListener(OnEdit);
		}

		private void OnDestroy()
		{
			if (inputField)
			{
				inputField.onEndEdit.RemoveAllListeners();
			}
		}
	}
}