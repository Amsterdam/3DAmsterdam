using Amsterdam3D.Sharing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface.Sharing
{
	public class ShareDialog : MonoBehaviour
	{
		public enum SharingState
		{
			SHARING_OPTIONS,
			SHARING_SCENE,
			SHOW_URL
		}


		[SerializeField]
		private RectTransform shareOptions;

		[SerializeField]
		private RectTransform progressFeedback;
		[SerializeField]
		private ProgressBar progressBar;

		[SerializeField]
		private RectTransform generatedURL;
		[SerializeField]
		private InputField generatedURLInputField;
		private string generatedURLAddress = "";

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
		}

		public void GenerateURL()
		{
			//Start uploading scene settings, and models to unique URL/session ID
			StartCoroutine(Share());
		}

		private IEnumerator Share()
		{
			progressBar.SetMessage("Instellingen opslaan..");
			progressBar.Percentage(0.2f);
			ChangeState(SharingState.SHARING_SCENE);

			var jsonScene = JsonUtility.ToJson(sceneSerializer.ToDataStructure(), true);
			//SERVER: post basic scene, and get unique token in return
			//SERVER: use token to upload model, one by one

			progressBar.SetMessage("Objecten opslaan..");
			while(sceneSerializer.CustomMeshIndex < sceneSerializer.CustomMeshCount-1)
			{
				progressBar.SetMessage("Objecten opslaan.. " + (sceneSerializer.CustomMeshIndex + 1) + "/" + sceneSerializer.CustomMeshCount);
				var jsonCustomObject = JsonUtility.ToJson(sceneSerializer.SerializeCustomObject(), true);
				Debug.Log(jsonCustomObject);
				progressBar.Percentage(0.3f + 0.7f * ((float)sceneSerializer.CustomMeshIndex / (float)sceneSerializer.CustomMeshCount - 1));
				yield return new WaitForSeconds(0.2f);				
			}

			//SERVER: Finalize and place json file
			progressBar.Percentage(1.0f);

			yield return new WaitForSeconds(0.1f);

			Debug.Log(jsonScene);

			/*progressBar.SetMessage("Instellingen opslaan..");
            progressBar.Percentage(0.2f);
            ChangeState(SharingState.SHARING_SCENE);

            yield return new WaitForSeconds(1.0f);
            progressBar.SetMessage("Object A wordt geupload..");
            progressBar.Percentage(0.5f);

            yield return new WaitForSeconds(2.0f);
            progressBar.SetMessage("Object B wordt geupload..");
            progressBar.Percentage(0.8f);

            yield return new WaitForSeconds(2.0f);
            progressBar.Percentage(1.0f);

            yield return new WaitForSeconds(0.1f);*/

			//Temp fake random URL
			generatedURLAddress = "https://3d.amsterdam.nl/?view=" + Path.GetRandomFileName().Split('.')[0];
			generatedURLInputField.text = generatedURLAddress;

			ChangeState(SharingState.SHOW_URL);
			yield return null;
		}

		public void ChangeState(SharingState newState)
		{
			switch (newState)
			{
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