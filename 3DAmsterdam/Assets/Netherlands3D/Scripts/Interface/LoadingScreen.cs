using Netherlands3D.Events;
using Netherlands3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface
{
	public class LoadingScreen : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI textMessage;

		[SerializeField]
		private ProgressBar progressBar;
		public ProgressBar ProgressBar { get => progressBar; }

		public static LoadingScreen Instance; //TODO: remove singleton, and make all users use events instead.

		[Header("Listeners")]
		[SerializeField]
		private FloatEvent onProgressBarPercentage;
		[SerializeField]
		private FloatEvent onProgressBarNormalisedValue;
		[SerializeField]
		private StringEvent onProgressBarMessage;
		[SerializeField]
		private StringEvent onProgressBarDetailedMessage;
		private void Awake()
		{
			Instance = this;

			if (onProgressBarMessage) onProgressBarMessage.AddListenerStarted(SetProgressBarMessage);
			if(onProgressBarDetailedMessage) onProgressBarDetailedMessage.AddListenerStarted(SetProgressBarDetailedMessage);
			SetProgressBarMessage("");
			SetProgressBarDetailedMessage("");

			if(onProgressBarPercentage) onProgressBarPercentage.AddListenerStarted(SetProgressBarPercentage);
			if(onProgressBarNormalisedValue) onProgressBarNormalisedValue.AddListenerStarted(SetProgressBarNormalisedValue);
		}

		private void SetProgressBarDetailedMessage(string message)
		{
			progressBar.SetMessage(message);
		}

		private void SetProgressBarMessage(string message)
		{
			textMessage.text = message;
		}

		public void SetProgressBarPercentage(float percentage)
		{
			SetProgressBarNormalisedValue((percentage>0) ? (percentage / 100.0f) : 0);
		}
		public void SetProgressBarNormalisedValue(float value)
		{
			progressBar.Percentage(value);

			if(value > 0 && value != 1)
            {
				Show();
			}
            else
            {
				Hide();
			}
		}

		public void ShowMessage(string text)
		{
			this.transform.SetAsLastSibling(); //Always on top
			gameObject.SetActive(true);
			textMessage.text = text;
		}
		public void Hide()
		{
			gameObject.SetActive(false);
		}
		public void Show()
		{
			gameObject.SetActive(true);
		}
	}
}