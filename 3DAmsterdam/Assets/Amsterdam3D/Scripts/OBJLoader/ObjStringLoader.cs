using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

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
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/Marineterrein_OBJ/25052020 MV 3D Model Marineterrein.obj"),
				File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/Marineterrein_OBJ/25052020 MV 3D Model Marineterrein.mtl")
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
#endif
		public void SetOBJFileName(string fileName)
		{
			objModelName = Path.GetFileNameWithoutExtension(fileName);
		}
		public void LoadOBJFromJavascript()
		{
			StartCoroutine(ParseOBJFromString(JavascriptMethodCaller.FetchOBJDataAsString(), JavascriptMethodCaller.FetchMTLDataAsString()));
		}
		private IEnumerator ParseOBJFromString(string objText, string mtlText = "")
		{
			//Display loading message covering entire screen
			loadingObjScreen.ShowMessage("Loading " + objModelName + "...");
			yield return new WaitForSeconds(0.1f);

			var newOBJ = new GameObject().AddComponent<ObjLoad>();
			if (mtlText != "")
				newOBJ.SetMaterialData(mtlText);

			newOBJ.SetGeometryData(objText);

			newOBJ.Build(defaultLoadedObjectsMaterial);
			newOBJ.name = objModelName;

			customObjectPlacer.AtPointer(newOBJ.gameObject);

			//hide panel after loading
			gameObject.SetActive(false);
			loadingObjScreen.Hide();

			yield return false;
		}
	}
}