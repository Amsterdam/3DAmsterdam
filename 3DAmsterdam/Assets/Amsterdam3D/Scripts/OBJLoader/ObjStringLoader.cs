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
		/*private void Update()
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
		}*/
#endif
		public void StartObjectParsing(string fileName)
		{
			objModelName = Path.GetFileNameWithoutExtension(fileName);
			newObjParser = new GameObject().AddComponent<ObjLoad>();
			newObjParser.name = objModelName;
			loadingObjScreen.ShowMessage("Objecten worden geladen...");
		}
		public void ShowProgress(int progress)
		{
			loadingObjScreen.ProgressBar.SetMessage(progress + " lines parsed");
			loadingObjScreen.ProgressBar.Percentage(Random.value);
		}

		public void ParseOBJChunk(string lines)
		{
			newObjParser.ParseNextObjLines(ref lines);
		}
		public void ParseMTLChunk(string lines)
		{
			newObjParser.ParseNextMtlLines(ref lines);
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
	}
}