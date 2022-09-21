using Netherlands3D.Events;
using Netherlands3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	public class LoadingScreen : MonoBehaviour
	{
		[SerializeField]
		private Text textMessage;

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

			if(onProgressBarMessage) onProgressBarMessage.started.AddListener(SetProgressBarMessage);
			if(onProgressBarPercentage) onProgressBarPercentage.started.AddListener(SetProgressBarPercentage);
			if(onProgressBarNormalisedValue) onProgressBarNormalisedValue.started.AddListener(SetProgressBarNormalisedValue);
			if(onProgressBarDetailedMessage) onProgressBarDetailedMessage.started.AddListener(SetProgressBarDetailedMessage);
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
			SetProgressBarPercentage(percentage / 100.0f);
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