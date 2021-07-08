using Netherlands3D.JavascriptConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	public class SelectableText : MonoBehaviour, IPointerEnterHandler
	{
		private InputField inputFieldSource;
		private InputField inputField;

		private Text text;
		private Text inputFieldText;

		void Start()
		{
			text = this.GetComponent<Text>();
			text.raycastTarget = true;
		}

		public void SetFieldPrefab(InputField inputFieldPrefab)
		{
			inputFieldSource = inputFieldPrefab;
			//Make sure our mouse pointer turns into a text pointer when hovering, letting the user know its selectable
			gameObject.AddComponent<ChangePointerStyleHandler>().StyleOnHover = ChangePointerStyleHandler.Style.TEXT;
		}

		private void FinishedSelectingText(string newString = "")
		{
			inputField.gameObject.SetActive(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!inputField && text.text.Length > 0)
			{
				CreateInputFieldCopy();
			}
			inputField.text = text.text;
			inputField.gameObject.SetActive(true);
		}

		private void CreateInputFieldCopy()
		{
			inputField = Instantiate(inputFieldSource, this.transform);
			inputFieldText = inputField.GetComponentInChildren<Text>();

			//Match visuals of textfield
			inputFieldText.font = text.font;
			inputFieldText.fontSize = text.fontSize;
			inputFieldText.fontStyle = text.fontStyle;

			inputField.onEndEdit.AddListener(FinishedSelectingText);
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