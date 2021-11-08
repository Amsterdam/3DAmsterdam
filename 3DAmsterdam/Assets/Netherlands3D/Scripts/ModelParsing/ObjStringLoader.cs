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
			string newmtlstring = Path.GetFileName(pathMtl);
			if (!File.Exists(pathMtl))
			{
				newmtlstring = "";
			}
			else
            {
				
				File.Copy(pathMtl, Application.persistentDataPath + "/" + newmtlstring, true);
			}

			string newobjstring = Path.GetFileName(pathObj);
			if (File.Exists(pathObj))
			{
				File.Copy(pathObj, Application.persistentDataPath + "/" + newobjstring,true);
				Debug.Log(Application.persistentDataPath + "/" + newobjstring);
			}

			StartCoroutine(ParseOBJfromStream(newobjstring, newmtlstring, Finished));
		}
		public void Finished(bool value)
        {
			print("Finished stream reading OBJ");
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
			// Read the obj-file
			SetOBJFileName(objFilePath);
			var objstreamReader =new GameObject().AddComponent<StreamreadOBJ>();
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
			ReadMTL mtlreader = objstreamReader.gameObject.AddComponent<ReadMTL>();
			// read the mtl-file
			if (File.Exists(Application.persistentDataPath + "/" + mtlFilePath))
			{
				string mtldata = File.ReadAllText(Application.persistentDataPath + "/" + mtlFilePath);
				
				isBusy = true;
				mtlreader.StartMTLParse(ref mtldata);
				while (isBusy)
				{
					isBusy = mtlreader.isBusy;
					yield return null;
				}
				File.Delete(Application.persistentDataPath + "/" + mtlFilePath);
			}

			objstreamReader.materialDataSlots = mtlreader.GetMaterialData();
			objstreamReader.CreateGameObject(defaultLoadedObjectsMaterial);
			isBusy = true;
			while (isBusy)
            {
				isBusy = !objstreamReader.isFinished;
				yield return null;
			}

			GameObject createdGO = objstreamReader.createdGameObject;
			createdGO.name = objModelName;
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
			loadingObjScreen.Hide();
			
			callback(objstreamReader.succes);
			Destroy(objstreamReader);

			yield return null;
        }
	}
}