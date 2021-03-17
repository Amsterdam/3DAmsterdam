using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Netherlands3D.JavascriptConnection;
using UnityEngine.Events;
using Netherlands3D.ObjectInteraction;

namespace Netherlands3D.ModelParsing
{
	public class ObjStringLoader : MonoBehaviour
	{
		[SerializeField]
		private Material defaultLoadedObjectsMaterial;

		[SerializeField]
		private LoadingScreen loadingObjScreen;

		[SerializeField]
		private UnityEvent doneLoadingModel;

		[SerializeField]
		private PlaceCustomObject customObjectPlacer;

		private string objModelName = "model";

		[SerializeField]
		private int maxLinesPerFrame = 200000; //20000 obj lines are close to a 4mb obj file

		private void Start()
		{
			loadingObjScreen.Hide();
		}

#if UNITY_EDITOR
		/// <summary>
		/// For Editor testing only.
		/// This method loads a obj and a mtl file.
		/// </summary>
		[ContextMenu("Load test models")]
		private void LoadTestModels()
		{
			if (!Application.isPlaying) return;
				StartCoroutine(ParseOBJFromString(
					File.ReadAllText(Application.dataPath + "/../TestModels/house.obj"),
					File.ReadAllText(Application.dataPath + "/../TestModels/house.mtl")
				));

		}
#endif
		/// <summary>
		/// Allows setting the model file name from Javascript and opening the loading screen
		/// </summary>
		/// <param name="fileName">The OBJ filename to be shown in the loading screen</param>
		public void SetOBJFileName(string fileName)
		{
			objModelName = Path.GetFileNameWithoutExtension(fileName);
			loadingObjScreen.ProgressBar.SetMessage("0%");
			loadingObjScreen.ProgressBar.Percentage(0);
			loadingObjScreen.ShowMessage("Model wordt geladen: " + objModelName);
		}

		/// <summary>
		/// Start the parsing of the OBJ, fetching the obj and mtl strings from Javascript
		/// </summary>
		public void LoadOBJFromJavascript()
		{
			StartCoroutine(ParseOBJFromString(JavascriptMethodCaller.FetchOBJDataAsString(), JavascriptMethodCaller.FetchMTLDataAsString()));
		}

		/// <summary>
		/// Start the parsing of OBJ and MTL strings
		/// </summary>
		/// <param name="objText">The OBJ string data</param>
		/// <param name="mtlText">The MTL string data</param>
		/// <returns></returns>
		private IEnumerator ParseOBJFromString(string objText, string mtlText = "")
		{
			//Create a new gameobject that parses OBJ lines one by one
			var newOBJLoader = new GameObject().AddComponent<ObjLoad>();
			float remainingLinesToParse;
			float totalLines;
			float percentage;

			//Parse the mtl file, filling our material library
			if (mtlText != "")
			{
				newOBJLoader.SetMaterialData(ref mtlText);
				remainingLinesToParse = newOBJLoader.ParseNextMtlLines(1);
				totalLines = remainingLinesToParse;

				loadingObjScreen.ShowMessage("Materialen worden geladen...");
				while (remainingLinesToParse > 0)
				{
					remainingLinesToParse = newOBJLoader.ParseNextMtlLines(maxLinesPerFrame);
					percentage = 1.0f - (remainingLinesToParse / totalLines);
					loadingObjScreen.ProgressBar.Percentage(percentage/100.0f); //Show first percent
					yield return null;
				}
			}

			//Parse the obj line by line
			newOBJLoader.SetGeometryData(ref objText);
			loadingObjScreen.ShowMessage("Objecten worden geladen...");

			var parsingSucceeded = true;
			remainingLinesToParse = newOBJLoader.ParseNextObjLines(1);
			totalLines = remainingLinesToParse;
			while (remainingLinesToParse > 0)
			{
				remainingLinesToParse = newOBJLoader.ParseNextObjLines(maxLinesPerFrame);
				if (remainingLinesToParse == -1)
				{
					//Failed to parse the line. Probably not a triangulated OBJ
					parsingSucceeded = false;
					JavascriptMethodCaller.Alert("Het is niet gelukt dit model te importeren.\nZorg dat de OBJ is opgeslagen met 'Triangulated' als instelling.");
				}
				else
				{
					percentage = 1.0f - (remainingLinesToParse / totalLines);
					loadingObjScreen.ProgressBar.SetMessage(Mathf.Round(percentage * 100.0f) + "%");
					loadingObjScreen.ProgressBar.Percentage(percentage);
				}
				yield return null;
			}
			if (parsingSucceeded)
			{
				newOBJLoader.Build(defaultLoadedObjectsMaterial);

				//Make interactable
				newOBJLoader.transform.localScale = new Vector3(1.0f, 1.0f, -1.0f);
				newOBJLoader.name = objModelName;
				newOBJLoader.gameObject.AddComponent<Transformable>();
				newOBJLoader.gameObject.AddComponent<MeshCollider>().sharedMesh = newOBJLoader.GetComponent<MeshFilter>().sharedMesh;
				newOBJLoader.gameObject.AddComponent<ClearMeshAndMaterialsOnDestroy>();
				customObjectPlacer.PlaceExistingObjectAtPointer(newOBJLoader.gameObject);
			}
			//hide panel and loading screen after loading
			loadingObjScreen.Hide();

			//Invoke done event
			doneLoadingModel.Invoke();

			//Remove this loader from finished object
			Destroy(newOBJLoader);
		}
	}
}