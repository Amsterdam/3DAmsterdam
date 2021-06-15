using Netherlands3D.JavascriptConnection;
using Netherlands3D.Sharing;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Sharing
{
	public class ShareDialog : MonoBehaviour
	{
		public enum SharingState
		{
			SHARING_OPTIONS,
			SHARING_SCENE,
			SHOW_URL,
			SERVER_PROBLEM
		}

		[SerializeField]
		private RectTransform shareOptions;

		[SerializeField]
		private Toggle editAllowToggle;

		[SerializeField]
		private RectTransform progressFeedback;
		[SerializeField]
		private ProgressBar progressBar;

		[SerializeField]
		private RectTransform generatedURL;

		[SerializeField]
		private SceneSerializer sceneSerializer;

		private SharingState state = SharingState.SHARING_OPTIONS;

		void OnEnable()
		{
			ChangeState(SharingState.SHARING_OPTIONS);
		}

		private void OnDisable()
		{
			StopAllCoroutines();
			JavascriptMethodCaller.ShowUniqueShareToken(false);
		}

		/// <summary>
		/// Start uploading scene settings and models to a unique URL/session ID file.
		/// </summary>
		public void GenerateURL()
		{
			StartCoroutine(Share());
		}

		/// <summary>
		/// The complete upload bar progress.
		/// </summary>
		private IEnumerator Share()
		{
			progressBar.SetMessage("Instellingen opslaan..");

			ChangeState(SharingState.SHARING_SCENE);
			yield return new WaitForEndOfFrame();

			string guid = Guid.NewGuid().ToString().Split('-')[0];

			var sceneAndMeshes = new SerializableSceneAndMeshes();
			sceneAndMeshes.SerializableScene = sceneSerializer.SerializeScene(editAllowToggle.isOn);
			sceneAndMeshes.Meshes = sceneSerializer.GetMeshes();

			var data = BinarySerializer.ObjectToByteArray(sceneAndMeshes);

			UnityWebRequest sceneSaveRequestObject = UnityWebRequest.Put(Config.activeConfiguration.shareUploadURL + guid, data);
			sceneSaveRequestObject.SendWebRequest();

			while (!sceneSaveRequestObject.isDone)
			{
				progressBar.Percentage(sceneSaveRequestObject.uploadProgress );					
				yield return null;
			}

            if (sceneSaveRequestObject.isNetworkError || sceneSaveRequestObject.isHttpError )
            {
                ChangeState(SharingState.SERVER_PROBLEM);
                yield break;
            }

            progressBar.Percentage(1.0f);
			yield return new WaitForSeconds(0.1f);

			ChangeState(SharingState.SHOW_URL);
			Debug.Log(Config.activeConfiguration.sharingViewUrl + guid);
			JavascriptMethodCaller.ShowUniqueShareToken(true, guid);
			yield return null;

		}

		/// <summary>
		/// Changes the state our sharing panel is in.
		/// </summary>
		/// <param name="newState">What sharing state should the panel be in.</param>
		public void ChangeState(SharingState newState)
		{
			state = newState;
			switch (newState)
			{
				case SharingState.SERVER_PROBLEM:
					WarningDialogs.Instance.ShowNewDialog();
					gameObject.SetActive(false);
					break;
				case SharingState.SHARING_OPTIONS:
					shareOptions.gameObject.SetActive(true);

					progressFeedback.gameObject.SetActive(false);
					generatedURL.gameObject.SetActive(false);
					break;
				case SharingState.SHARING_SCENE:
					progressFeedback.gameObject.SetActive(true);

					shareOptions.gameObject.SetActive(false);
					generatedURL.gameObject.SetActive(false);
					break;
				case SharingState.SHOW_URL:
					generatedURL.gameObject.SetActive(true);

					shareOptions.gameObject.SetActive(false);
					progressFeedback.gameObject.SetActive(false);
					break;
				default:
					break;
			}
		}
	}
}