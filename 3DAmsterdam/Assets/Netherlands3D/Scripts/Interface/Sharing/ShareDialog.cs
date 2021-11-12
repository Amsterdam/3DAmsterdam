using Netherlands3D.JavascriptConnection;
using Netherlands3D.Logging;
using Netherlands3D.Sharing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

		/// <summary>
		/// JS plugins are the *.jslib files found in the project.
		/// </summary>
		[DllImport("__Internal")]
		private static extern void IndexedDBUpload(string fileName,string targetURL);

		[SerializeField]
		private RectTransform shareOptions;

		[SerializeField]
		private Toggle editAllowToggle;

		[SerializeField]
		private RectTransform progressFeedback;
		[SerializeField]
		private ProgressBar progressBar;

		[SerializeField]
		private SharedURL sharedURL;

		[SerializeField]
		private SceneSerializer sceneSerializer;

		private SharingState state = SharingState.SHARING_OPTIONS;

		private ServerReturn currentSceneServerReturn;
		private int modelUploadsRemaining = 0;
		private bool waitingForIndexedDBUpload = false;

		void OnEnable()
		{
			ChangeState(SharingState.SHARING_OPTIONS);
		}

		private void OnDisable()
		{
			StopAllCoroutines();
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
			progressBar.Percentage(0.2f);

			ChangeState(SharingState.SHARING_SCENE);
			yield return new WaitForEndOfFrame(); 
			var jsonScene = JsonUtility.ToJson(sceneSerializer.SerializeScene(editAllowToggle.isOn), true);
			print(jsonScene);

			print("Save scene using url:" + Config.activeConfiguration.sharingUploadScenePath);
			//Post basic scene, and optionaly get unique tokens in return
			UnityWebRequest sceneSaveRequest = UnityWebRequest.Put(Config.activeConfiguration.sharingUploadScenePath, jsonScene);
			sceneSaveRequest.SetRequestHeader("Content-Type", "application/json");
			yield return sceneSaveRequest.SendWebRequest();

			if (sceneSaveRequest.result != UnityWebRequest.Result.Success)
			{
				print(sceneSaveRequest.downloadHandler.text);
				ChangeState(SharingState.SERVER_PROBLEM);
				yield break;
			}
			else
			{
				//Check if we got some tokens for model upload, and download them 1 at a time.
				currentSceneServerReturn = JsonUtility.FromJson<ServerReturn>(sceneSaveRequest.downloadHandler.text);
				sceneSerializer.sharedSceneId = currentSceneServerReturn.sceneId;
				Debug.Log("Scene return: " + sceneSaveRequest.downloadHandler.text);

				var totalVerts = 0;

				if (currentSceneServerReturn.modelUploadTokens.Length > 0)
				{
					modelUploadsRemaining = currentSceneServerReturn.modelUploadTokens.Length;
					progressBar.SetMessage("Objecten opslaan..");
					progressBar.Percentage(0.3f);
					while (modelUploadsRemaining >= currentSceneServerReturn.modelUploadTokens.Length)
					{
						int currentModelIndex = modelUploadsRemaining - currentSceneServerReturn.modelUploadTokens.Length;
						progressBar.SetMessage("Objecten opslaan.. " + (currentModelIndex + 1) + "/" + currentSceneServerReturn.modelUploadTokens.Length);
						var pathToLocalBinaryFile = sceneSerializer.SerializeCustomObject(currentModelIndex, currentSceneServerReturn.sceneId, currentSceneServerReturn.modelUploadTokens[currentModelIndex].token);
						var putPath = Config.activeConfiguration.sharingUploadModelPath.Replace("{sceneId}", currentSceneServerReturn.sceneId).Replace("{modelToken}", currentSceneServerReturn.modelUploadTokens[currentModelIndex].token);

#if UNITY_WEBGL && !UNITY_EDITOR
						Debug.Log("Preparing IndexedDB: " + putPath);
						waitingForIndexedDBUpload = true;
						UploadFromIndexedDB(pathToLocalBinaryFile, putPath);
						yield return new WaitWhile(() => waitingForIndexedDBUpload); 
						NextModelUpload();
						yield return new WaitForSeconds(0.2f);
#else
						UnityWebRequest modelSaveRequest = UnityWebRequest.Put(putPath, File.ReadAllBytes(pathToLocalBinaryFile));
						yield return modelSaveRequest.SendWebRequest();
						if (modelSaveRequest.result != UnityWebRequest.Result.Success)
						{
							ChangeState(SharingState.SERVER_PROBLEM);
							yield break;
						}
						else
						{	
							NextModelUpload();
							yield return new WaitForSeconds(0.2f);
						}
#endif
					}
				}
				yield return CompleteSharing();
			}
		}

		public void NextModelUpload()
		{
			modelUploadsRemaining--;
			var currentModelLoadPercentage = 1-(currentSceneServerReturn.modelUploadTokens.Length / (float)modelUploadsRemaining);
			progressBar.Percentage(0.3f + (0.7f * currentModelLoadPercentage));
		}

		public void IndexedDBUploadCompleted()
		{
			modelUploadsRemaining--;
			waitingForIndexedDBUpload = false;
		}
		public void IndexedDBUploadFailed()
		{
			ChangeState(SharingState.SERVER_PROBLEM);
		}

		private IEnumerator CompleteSharing()
		{
			//Let analytics know we saved a scene, with the amount of objects and vertex count
			Analytics.SendEvent("ShareScene", "Shared", $"Objects:{currentSceneServerReturn.modelUploadTokens.Length}");

			//Make sure the progressbar shows 100% before jumping to the next state
			progressBar.Percentage(1.0f);
			yield return new WaitForSeconds(0.1f);

			ChangeState(SharingState.SHOW_URL);

			var sharedSceneURL = Config.activeConfiguration.sharingViewScenePath.Replace("{sceneId}", currentSceneServerReturn.sceneId);
			if (!sharedSceneURL.Contains("https://") && !sharedSceneURL.Contains("http://"))
			{
				//Use relative path
				sharedSceneURL = Application.absoluteURL + Config.activeConfiguration.sharingViewScenePath.Replace("{sceneId}", currentSceneServerReturn.sceneId);
			}

			sharedURL.ShowURL(sharedSceneURL);

			JavascriptMethodCaller.SetUniqueShareURLToken(currentSceneServerReturn.sceneId);

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
					sharedURL.gameObject.SetActive(false);
					break;
				case SharingState.SHARING_SCENE:
					progressFeedback.gameObject.SetActive(true);

					shareOptions.gameObject.SetActive(false);
					sharedURL.gameObject.SetActive(false);
					break;
				case SharingState.SHOW_URL:
					sharedURL.gameObject.SetActive(true);

					shareOptions.gameObject.SetActive(false);
					progressFeedback.gameObject.SetActive(false);
					break;
				default:
					break;
			}
		}
	}
}