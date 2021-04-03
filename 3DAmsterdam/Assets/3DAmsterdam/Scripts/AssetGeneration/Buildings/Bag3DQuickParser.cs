using ConvertCoordinates;
using Netherlands3D.AssetGeneration.CityJSON;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class Bag3DQuickParser : MonoBehaviour
{
    [SerializeField]
    private string jsonsSourceFolder = "C:/Users/Sam/Desktop/downloaded_tiles_amsterdam/";

    [SerializeField]
    private int lod = 22;

	[SerializeField]
	private int lodSlot = 2;

    private List<Vector3> allVerts;
    private List<int> meshTriangles;

	private int vertIndex;

	private void Start()
    {
        StartCoroutine(TraverseTheFiles());
    }
    private IEnumerator TraverseTheFiles()
    {
        //Read files list 
        var info = new DirectoryInfo(jsonsSourceFolder);
        var fileInfo = info.GetFiles();

        //First create gameobjects for all the buildigns we parse
        int parsed = 0;
        for (int i = 0; i < fileInfo.Length; i++)
        {
            var file = fileInfo[i];
            Debug.Log("Parsing " + file.Name);
            yield return new WaitForEndOfFrame();

			var jsonstring = File.ReadAllText(file.FullName);
			var cityjsonNode = JSON.Parse(jsonstring);

			//get vertices
			allVerts = new List<Vector3>();

			//optionaly parse transform scale and offset
			var transformScale = (cityjsonNode["transform"] != null && cityjsonNode["transform"]["scale"] != null) ? new Vector3Double(
				cityjsonNode["transform"]["scale"][0].AsDouble,
				cityjsonNode["transform"]["scale"][1].AsDouble,
				cityjsonNode["transform"]["scale"][2].AsDouble
			) : new Vector3Double(1, 1, 1);

			var transformOffset = (cityjsonNode["transform"] != null && cityjsonNode["transform"]["translate"] != null) ? new Vector3Double(
				   cityjsonNode["transform"]["translate"][0].AsDouble,
				   cityjsonNode["transform"]["translate"][1].AsDouble,
				   cityjsonNode["transform"]["translate"][2].AsDouble
			) : new Vector3Double(0, 0, 0);

			//now load all the vertices with the scaler and offset applied
			foreach (JSONNode node in cityjsonNode["vertices"])
			{
				var rd = new Vector3RD(
						node[0].AsDouble * transformScale.x + transformOffset.x,
						node[1].AsDouble * transformScale.y + transformOffset.y,
						node[2].AsDouble * transformScale.z + transformOffset.z
				);
				var unityCoordinates = CoordConvert.RDtoUnity(rd);
				allVerts.Add(unityCoordinates);
			}

			//now build the meshes and create objects for these buildings
			foreach (JSONNode buildingNode in cityjsonNode["CityObjects"])
			{
				var name = buildingNode["attributes"]["identificatie"].Value.Replace("NL.IMBAG.Pand.", "");
				GameObject building = new GameObject();
				building.transform.SetParent(this.transform, false);
				building.name = name;
				var boundaries = buildingNode["geometry"][lodSlot]["boundaries"][0];
				meshTriangles = new List<int>();
				List<Vector3> thisMeshVerts = new List<Vector3>();

				foreach (JSONNode boundary in boundaries)
				{
					JSONNode triangle = boundary[0];

					vertIndex = triangle[2].AsInt;
					thisMeshVerts.Add(allVerts[vertIndex]);
					meshTriangles.Add(thisMeshVerts.Count - 1); //TODO. Group same verts

					vertIndex = triangle[1].AsInt;
					thisMeshVerts.Add(allVerts[vertIndex]);
					meshTriangles.Add(thisMeshVerts.Count - 1);

					vertIndex = triangle[0].AsInt;
					thisMeshVerts.Add(allVerts[vertIndex]);
					meshTriangles.Add(thisMeshVerts.Count - 1);
				}

				//Construct the mesh
				Mesh buildingMesh = new Mesh();
				buildingMesh.vertices = thisMeshVerts.ToArray();
				buildingMesh.triangles = meshTriangles.ToArray();
				buildingMesh.RecalculateNormals();

				building.AddComponent<MeshRenderer>().enabled = false;
				building.AddComponent<MeshFilter>().sharedMesh = buildingMesh;
			}
			parsed++;
			yield return new WaitForEndOfFrame();
		}
    }
}
