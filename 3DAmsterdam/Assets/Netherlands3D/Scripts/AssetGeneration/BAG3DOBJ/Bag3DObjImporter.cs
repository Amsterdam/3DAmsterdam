using ConvertCoordinates;
using Netherlands3D;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public class Bag3DObjImporter : MonoBehaviour
{
	[SerializeField]
	private string bag3DSourceFilesFolder = "";
	[SerializeField]
	private string filter = "*.obj";

	[SerializeField]
	private string prefix = "NL.IMBAG.Pand.";

	private ObjLoad objLoader;

	[SerializeField]
	private int parsePerFrame = 1000;

	[SerializeField]
	private Material applyMaterial;

	[SerializeField]
	private GameObject enableOnFinish;

	[System.Serializable]
	public class RemapObjectNames
	{
		public string sourceName = "";
		public string newName = "";
	}

	private void Start()
	{
		StartCoroutine(ParseSpecificFiles());
	}

	private IEnumerator ParseSpecificFiles()
	{
		//Read files list 
		var info = new DirectoryInfo(bag3DSourceFilesFolder);
		var fileInfo = info.GetFiles(filter);
		print("Found " + fileInfo.Length + " obj files.");
		//First create gameobjects for all the buildings we parse
		for (int i = 0; i < fileInfo.Length; i++)
		{
			var file = fileInfo[i];
			print(i + "/" + fileInfo.Length + " " + file.Name);

			var objString = File.ReadAllText(file.FullName);

			//Start a new ObjLoader
			if (objLoader) Destroy(objLoader);
			objLoader = this.gameObject.AddComponent<ObjLoad>();
			//objLoader.ObjectUsesRDCoordinates = true; //automatic

			objLoader.IgnoreObjectsOutsideOfBounds = true;
			objLoader.BottomLeftBounds = Config.activeConfiguration.MinBoundingBox;
			objLoader.TopRightBounds = Config.activeConfiguration.MaxBoundingBox;
			objLoader.MaxSubMeshes = 1;
			objLoader.SplitNestedObjects = true;
			objLoader.WeldVertices = true;
			objLoader.SetGeometryData(ref objString);

			var objLinesToBeParsed = 100;
			while (objLinesToBeParsed > 0)
			{
				objLinesToBeParsed = objLoader.ParseNextObjLines(parsePerFrame);
				yield return new WaitForEndOfFrame();
			}
			objLoader.Build(applyMaterial);
			print(i + "/" + fileInfo.Length + " " + file.Name + " done.");
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForEndOfFrame();
		RenameObjects();

		if (enableOnFinish) enableOnFinish.SetActive(true);
	}

	private void RenameObjects()
	{	
		print("Added all gameobjects from obj file.");
		foreach (Transform child in transform)
		{
			child.gameObject.name = child.gameObject.name.Replace(prefix, "");
		}
	}
}
