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
using Netherlands3D.Interface.SidePanel;
using static Netherlands3D.ObjectInteraction.Transformable;

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
		public Masking.RuntimeMask mask;

		[SerializeField]
		private PlaceCustomObject customObjectPlacer;

		private string objModelName = "model";

		[SerializeField]
		private int maxLinesPerFrame = 200000; //20000 obj lines are close to a 4mb obj file

		private Transformable transformable;

		private void Start()
		{
			loadingObjScreen.Hide();
		}

#if UNITY_EDITOR
		/// <summary>
		/// For Editor testing only.
		/// This method loads a obj and a mtl file.
		/// </summary>
		[ContextMenu("Open selection dialog")]
		public void OpenModelViaEditor()
		{
			if (!Application.isPlaying) return;

			string pathObj = UnityEditor.EditorUtility.OpenFilePanel("Open OBJ", "", "obj");
			string pathMtl = pathObj.Replace(".obj", ".mtl");
			if (!File.Exists(pathMtl))
			{
				pathMtl = "";
			}

			StartCoroutine(ParseOBJFromString(
					File.ReadAllText(pathObj),
					File.ReadAllText(pathMtl)
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
		/// Method to remove loading screen if somehow the import/loading was aborted
		/// </summary>
		public void AbortImport()
		{
			loadingObjScreen.Hide();
			ServiceLocator.GetService<WarningDialogs>().ShowNewDialog("U kunt maximaal één .obj tegelijk importeren met optioneel daarnaast een bijbehorend .mtl bestand.");
		}

		/// <summary>
		/// Start the parsing of OBJ and MTL strings
		/// </summary>
		/// <param name="objText">The OBJ string data</param>
		/// <param name="mtlText">The MTL string data</param>
		/// <returns></returns>
		private IEnumerator ParseOBJFromString(string objText, string mtlText = "")
		{
			//Too small or empty to be OBJ content? Abort, and give some explanation to the user.
			Debug.Log("OBJ length: " + objText.Length);
			if (objText.Length < 5)
			{
				AbortImport();
				yield break;
			}

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
				newOBJLoader.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				newOBJLoader.name = objModelName;
				
				newOBJLoader.gameObject.AddComponent<MeshCollider>().sharedMesh = newOBJLoader.GetComponent<MeshFilter>().sharedMesh;
				newOBJLoader.gameObject.AddComponent<ClearMeshAndMaterialsOnDestroy>();
				transformable = newOBJLoader.gameObject.AddComponent<Transformable>();
				transformable.madeWithExternalTool = true;
				transformable.mask = mask;

				if (newOBJLoader.ObjectUsesRDCoordinates==false)
                {
					if (transformable.placedTransformable == null) transformable.placedTransformable = new ObjectPlacedEvent();
					//transformable.placedTransformable.AddListener(RemapMaterials);
					customObjectPlacer.PlaceExistingObjectAtPointer(newOBJLoader.gameObject);
				}
				else
                {
					transformable.stickToMouse = false;
                }
				
			}
			//placementSettings();
			//hide panel and loading screen after loading
			loadingObjScreen.Hide();

			//Invoke done event
			doneLoadingModel.Invoke();
			

			//Remove this loader from finished object
			Destroy(newOBJLoader);
		}
	}
}