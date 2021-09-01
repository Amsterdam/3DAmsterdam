/* Copyright(C)  X Gemeente
                 X Amsterdam
                 X Economic Services Departments
Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License. You may obtain a copy of the License at:
https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions and limitations under the License.
*/

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


			string newobjstring = Path.GetFileName(pathObj);

			if (File.Exists(pathObj))
			{
				File.Copy(pathObj, Application.persistentDataPath + "/" + newobjstring,true);
				Debug.Log(Application.persistentDataPath + "/" + newobjstring);
			}

			//LoadOBJFromIndexedDB(new List<string>() { pathObj, pathMtl }, finished);
			StartCoroutine(ParseOBJfromStream(newobjstring, pathMtl, finished));

			//StartCoroutine(ParseOBJFromString(
			//		File.ReadAllText(pathObj),
			//		File.ReadAllText(pathMtl)
					
			//));
		}
		public void finished(bool value)
        {

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
			//StartCoroutine(ParseOBJFromString(JavascriptMethodCaller.FetchOBJDataAsString(), JavascriptMethodCaller.FetchMTLDataAsString()));
		}


		public void LoadOBJFromIndexedDB(List<string> filenames, System.Action<bool> callback)
        {



			Debug.Log(filenames.Count + " files received");
			string objstring = "";
			string mtlstring = "";
            for (int i = 0; i < filenames.Count; i++)
            {
				Debug.Log(filenames[i]);
				string extention = filenames[i].Substring(filenames[i].Length - 4);
                if (extention.IndexOf("obj")>-1)
                {
					objstring = filenames[i];

					
                }
				if (extention.IndexOf("mtl") > -1)
				{
					mtlstring = filenames[i];

				}

			}

			StartCoroutine(ParseOBJfromStream(objstring, mtlstring, callback));
        }

		/// <summary>
		/// Method to remove loading screen if somehow the import/loading was aborted
		/// </summary>
		public void AbortImport()
		{
			loadingObjScreen.Hide();
			WarningDialogs.Instance.ShowNewDialog("U kunt maximaal één .obj tegelijk importeren met optioneel daarnaast een bijbehorend .mtl bestand.");
		}


		private IEnumerator ParseOBJfromStream(string objFilePath, string mtlFilePath, System.Action<bool> callback)
        {
			var objstreamReader =new GameObject().AddComponent<StreamreadOBJ>();
			Debug.Log(objFilePath);
			objstreamReader.loadingObjScreen = loadingObjScreen;
			objstreamReader.ReadOBJ(objFilePath);
			bool isBusy = true;
			loadingObjScreen.ShowMessage("Objecten worden geladen...");
			while (isBusy)
            {
				isBusy = !objstreamReader.isFinished;
				yield return null;
            }
			Debug.Log("done reading");
			Debug.Log("readsucces = " + objstreamReader.succes.ToString());


			 objstreamReader.CreateGameObject(defaultLoadedObjectsMaterial);
			isBusy = true;
			while (isBusy)
            {
				isBusy = !objstreamReader.isFinished;
				yield return null;
			}

			GameObject createdGO = objstreamReader.createdGameObject;


			objstreamReader.Buffer = new GeometryBuffer(); ;
			//newOBJLoader.name = objModelName;

			createdGO.AddComponent<MeshCollider>().sharedMesh = createdGO.GetComponent<MeshFilter>().sharedMesh;
			createdGO.AddComponent<ClearMeshAndMaterialsOnDestroy>();
			transformable = createdGO.AddComponent<Transformable>();
			transformable.madeWithExternalTool = true;
			transformable.mask = mask;

			if (objstreamReader.ObjectUsesRDCoordinates == false)
			{
				if (transformable.placedTransformable == null) transformable.placedTransformable = new ObjectPlacedEvent();
				//transformable.placedTransformable.AddListener(RemapMaterials);
				customObjectPlacer.PlaceExistingObjectAtPointer(createdGO);
				
			}
			else
			{
				transformable.stickToMouse = false;
			}
			Destroy(objstreamReader);
			loadingObjScreen.Hide();
			
			callback(objstreamReader.succes);
			yield return null;
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