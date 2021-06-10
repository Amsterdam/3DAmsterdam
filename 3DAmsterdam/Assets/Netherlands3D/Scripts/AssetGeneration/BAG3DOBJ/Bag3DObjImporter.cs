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
	private Transform[] targetChildContainers;

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
		objLoader = this.gameObject.AddComponent<ObjLoad>();
		StartCoroutine(ParseSpecificFiles());
	}

	private IEnumerator ParseSpecificFiles()
	{
		//Read files list 
		var info = new DirectoryInfo(bag3DSourceFilesFolder);
		var fileInfo = info.GetFiles(filter);
		print("Found " + fileInfo.Length + " obj files.");
		//First create gameobjects for all the buildigns we parse
		int parsed = 0;
		for (int i = 0; i < fileInfo.Length; i++)
		{
			var file = fileInfo[i];
			print(parsed + "/" + fileInfo.Length + " " + file.Name);

			var objString = File.ReadAllText(file.FullName);

			//Start a new ObjLoader
			if (objLoader) Destroy(objLoader);
			objLoader = this.gameObject.AddComponent<ObjLoad>();
			//objLoader.ObjectUsesRDCoordinates = true; //automatic

			//Define unity coordinate rectangle bounding box
			var min = CoordConvert.RDtoUnity(new Vector3RD(Config.activeConfiguration.MinBoundingBox.x, Config.activeConfiguration.MinBoundingBox.y,0));
			var max = CoordConvert.RDtoUnity(new Vector3RD(Config.activeConfiguration.MaxBoundingBox.x, Config.activeConfiguration.MaxBoundingBox.y,0));

			var rect = new Rect();
			//Bottomleft
			rect.xMin = min.x;
			rect.yMin = min.y;
			//Topright
			rect.xMax = max.x;
			rect.yMax = max.y;

			objLoader.MaxBounds = rect;
			objLoader.MaxSubMeshes = 1;
			objLoader.SplitNestedObjects = true;
			objLoader.WeldVertices = true;
			objLoader.SetGeometryData(ref objString);

			var objLinesToBeParsed = 100;
			while (objLinesToBeParsed > 0)
			{
				objLinesToBeParsed = objLoader.ParseNextObjLines(parsePerFrame);
				print(objLinesToBeParsed + " obj lines remaining");
				yield return new WaitForEndOfFrame();
			}
			objLoader.Build(applyMaterial);

			yield return new WaitForEndOfFrame();
		}

		RemapObjectNamesAndCleanUp();

		if (enableOnFinish) enableOnFinish.SetActive(true);
	}

	private void RemapObjectNamesAndCleanUp()
	{	
		print("Filtering by specific object names and renaming");
		foreach (Transform child in transform)
		{
			//Copy this object into our target container
			var copy = Instantiate(child.gameObject, transform);
			copy.transform.position = child.position;
			copy.transform.rotation = child.rotation;
			copy.name = child.name.Replace(prefix, "");
		}
	}
}
