using Netherlands3D.Interface;
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
	}
}