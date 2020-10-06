using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Amsterdam3D.JavascriptConnection;

namespace Amsterdam3D.UserLayers
{
	public class ObjStringLoader : MonoBehaviour
	{
		[SerializeField]
		private Material defaultLoadedObjectsMaterial;

		[SerializeField]
		private LoadingScreen loadingObjScreen;

		[SerializeField]
		private PlaceCustomObject customObjectPlacer;

		private string objModelName = "model";


		[SerializeField]
		private int maxLinesPerFrame = 100;
		private float remainingLinesToParse;
		private float totalLines;
		private float percentage;

		private ObjLoad newObjParser;

		private string parsableChunk = "";

		private void Start()
		{
			loadingObjScreen.Hide();
		}

#if UNITY_EDITOR
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.L))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/KRZNoord_OBJ/Testgebied_3DAmsterdam.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/KRZNoord_OBJ/Testgebied_3DAmsterdam.mtl")
				));
			if (Input.GetKeyDown(KeyCode.K))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/SketchUp_OBJexport_triangulated/25052020 MV 3D Model Marineterrein.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/SketchUp_OBJexport_triangulated/25052020 MV 3D Model Marineterrein.mtl")
				));
			if (Input.GetKeyDown(KeyCode.H))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/suzanne.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/suzanne.mtl")
				));
			if (Input.GetKeyDown(KeyCode.J))
				StartCoroutine(ParseOBJFromString(
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/suzanne.obj"),
				""
				));
		}
		public void LoadOBJFromJavascript()
		{
			StartCoroutine(ParseOBJFromString(JavascriptMethodCaller.FetchOBJDataAsString(), JavascriptMethodCaller.FetchMTLDataAsString()));
		}
#endif
		public void StartObjectParsing(string fileName)
		{
			objModelName = Path.GetFileNameWithoutExtension(fileName);
			newObjParser = new GameObject().AddComponent<ObjLoad>();
			newObjParser.name = objModelName;
		}

		public void ParseOBJChunk(int progress)
		{
			parsableChunk = JavascriptMethodCaller.FetchPartialOBJDataAsString();
			newObjParser.SetGeometryData(ref parsableChunk);
			parsableChunk = "";

			loadingObjScreen.ShowMessage("Objecten worden geladen..." + progress);

			newObjParser.ParseNextObjLines();
		}
		public void ParseMTLChunk(int progress)
		{
			parsableChunk = JavascriptMethodCaller.FetchPartialMTLDataAsString(); 
			newObjParser.SetMaterialData(ref parsableChunk);
			parsableChunk = "";

			loadingObjScreen.ShowMessage("Materialen worden geladen..." + progress);

			newObjParser.ParseNextMtlLines();
		}

		public void FinishParsing()
		{
			newObjParser.Build(defaultLoadedObjectsMaterial);

			newObjParser.transform.Rotate(0, 90, 0);
			newObjParser.transform.localScale = new Vector3(1.0f, 1.0f, -1.0f);

			newObjParser.gameObject.AddComponent<Draggable>();
			newObjParser.gameObject.AddComponent<MeshCollider>().sharedMesh = newObjParser.GetComponent<MeshFilter>().sharedMesh;

			customObjectPlacer.PlaceExistingObjectAtPointer(newObjParser.gameObject);

			//hide panel and loading screen after loading
			loadingObjScreen.Hide();
		}

		private IEnumerator ParseOBJFromString(string objText, string mtlText = "")
		{
			//Display loading message covering entire screen
			loadingObjScreen.ShowMessage("Model wordt geladen: " + objModelName);
			yield return new WaitForEndOfFrame();
			yield return new WaitForSeconds(0.1f);

			//Create a new gameobject that parses OBJ lines one by one
			var newOBJ = new GameObject().AddComponent<ObjLoad>();	

			//Optionaly parse the mtl file first, filling our material library
			if (mtlText != "")
			{
				newOBJ.SetMaterialData(ref mtlText);
				remainingLinesToParse = newOBJ.ParseNextMtlLines(1);
				totalLines = remainingLinesToParse;

				loadingObjScreen.ShowMessage("Materialen worden geladen...");
				while (remainingLinesToParse > 0)
				{
					remainingLinesToParse = newOBJ.ParseNextMtlLines(maxLinesPerFrame);
					percentage = 1.0f - (remainingLinesToParse / totalLines);
					loadingObjScreen.ProgressBar.SetMessage(Mathf.Round(percentage * 100.0f) + "%");
					loadingObjScreen.ProgressBar.Percentage(percentage);
					yield return null;
				}
			}

			//Parse the obj line by line
			newOBJ.SetGeometryData(ref objText);
			loadingObjScreen.ShowMessage("Objecten worden geladen...");
			remainingLinesToParse = newOBJ.ParseNextObjLines(1);
			totalLines = remainingLinesToParse;
			while (remainingLinesToParse > 0)
			{
				remainingLinesToParse = newOBJ.ParseNextObjLines(maxLinesPerFrame);
				percentage = 1.0f - (remainingLinesToParse / totalLines);
				loadingObjScreen.ProgressBar.SetMessage(Mathf.Round(percentage * 100.0f) + "%");
				loadingObjScreen.ProgressBar.Percentage(percentage);
				yield return null;
			}
			newOBJ.Build(defaultLoadedObjectsMaterial);
			
			//Make interactable
			newOBJ.transform.Rotate(0, 90, 0);
			newOBJ.transform.localScale = new Vector3(1.0f, 1.0f, -1.0f);
;			newOBJ.name = objModelName;
			newOBJ.gameObject.AddComponent<Draggable>();
			newOBJ.gameObject.AddComponent<MeshCollider>().sharedMesh = newOBJ.GetComponent<MeshFilter>().sharedMesh;

			customObjectPlacer.PlaceExistingObjectAtPointer(newOBJ.gameObject);

			//hide panel and loading screen after loading
			loadingObjScreen.Hide();

			yield return false;
		}
	}
}