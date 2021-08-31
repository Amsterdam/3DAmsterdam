using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Districts : MonoBehaviour
{
	private string baseUrl = "https://geodata.nationaalgeoregister.nl/wijkenbuurten2020/wfs?service=WFS&version=2.0.0&request=GetFeature&outputFormat=json&typeName=wijkenbuurten2020:cbs_buurten_2020&propertyName=wijknaam,buurtnaam,geom&bbox="; //109000,474000,141000,501000 , 

	public string districtsFilepath = "D:/3DAmsterdam/wijken_amsterdam.txt";
	public string neighbourhoodsFilepath = "D:/3DAmsterdam/buurten_amsterdam.txt";
	public GameObject districtMarker;

	private List<District> districtNames = new List<District>();
	private List<District> neighbourhoodNames = new List<District>();

	private string latstring = "\"lat\": ";
	private string lonstring = "\"lon\": ";
	private string namestring = "\"name\": ";
	private int index = 0;

	[SerializeField]
	private float hideDistance = 300;

	[SerializeField]
	private float offsetFromGround = 30;

	[SerializeField]
	private float maxSpawnsPerFrame = 100;

	void Start()
	{
		//StartCoroutine(ReadFromFile());

		StartCoroutine(LoadDistrictsFromWFS());
	}

	private IEnumerator LoadDistrictsFromWFS()
	{
		string url = $"{baseUrl}{Config.activeConfiguration.BottomLeftRD.x},{Config.activeConfiguration.BottomLeftRD.y},{Config.activeConfiguration.TopRightRD.x},{Config.activeConfiguration.TopRightRD.y}";
		Debug.Log(url);

		var districtNamesRequest = UnityWebRequest.Get(url);
		yield return districtNamesRequest.SendWebRequest();

		if (districtNamesRequest.result == UnityWebRequest.Result.Success)
		{
			GeoJSON customJsonHandler = new GeoJSON(districtNamesRequest.downloadHandler.text);
			yield return null;
			Vector3 placementPoint;
			int parseCounter = 0;

			while (customJsonHandler.GotoNextFeature())
			{
				parseCounter++;
				if ((parseCounter % maxSpawnsPerFrame) == 0) yield return null;
				
				string districtName = customJsonHandler.getPropertyStringValue("wijknaam");
				string neighbourhoodName = customJsonHandler.getPropertyStringValue("buurtnaam");

				if (districtName.Length > 1)
				{
					List<double> multiPolygonCoordinates = customJsonHandler.getGeometryMultiPolygonString();
					placementPoint = CoordConvert.RDtoUnity(new Vector2RD(multiPolygonCoordinates[0], multiPolygonCoordinates[1]));
					placementPoint.y = offsetFromGround;

					GameObject newDistrictMarkerGameObject = Instantiate(districtMarker, transform);
					newDistrictMarkerGameObject.GetComponent<TMPro.TextMeshPro>().text = districtName;
					newDistrictMarkerGameObject.transform.position = CoordConvert.RDtoUnity(new Vector2RD(multiPolygonCoordinates[0], multiPolygonCoordinates[1]));
					districtNames.Add(newDistrictMarkerGameObject.GetComponent<District>());
				}
			}
			yield return null;
		}
	}

	IEnumerator ReadFromFile()
	{
		var districtsRequest = UnityWebRequest.Get($"{Config.activeConfiguration.webserverRootPath}{districtsFilepath}");
		yield return districtsRequest.SendWebRequest();

		if (districtsRequest.result == UnityWebRequest.Result.Success)
		{
			string districtsText = districtsRequest.downloadHandler.text;
			int nextIndex;
			int nameEndIndex;
			bool keepGoing = true;
			while (keepGoing)
			{
				nextIndex = districtsText.IndexOf(latstring, index) + latstring.Length;
				if (nextIndex < index)
				{
					keepGoing = false; break;
				}
				index = nextIndex;
				if (index == -1) { keepGoing = false; break; }
				double lat = StringManipulation.ParseNextDouble(districtsText, ',', index, out index);
				index = districtsText.IndexOf(lonstring, index) + lonstring.Length;
				if (index == -1) { keepGoing = false; break; }
				double lon = StringManipulation.ParseNextDouble(districtsText, ',', index, out index);
				index = districtsText.IndexOf(namestring, index) + namestring.Length + 1;
				if (index == -1) { keepGoing = false; break; }
				nameEndIndex = districtsText.IndexOf('"', index);
				string name = districtsText.Substring(index, nameEndIndex - index);
				index = nameEndIndex;
				if (index < 0) { keepGoing = false; break; }
				Vector3 coordinate = CoordConvert.WGS84toUnity(lon, lat);
				coordinate.y = Config.activeConfiguration.zeroGroundLevelY + offsetFromGround;

				GameObject newGameObject = Instantiate(districtMarker, transform);
				newGameObject.GetComponentInChildren<TMPro.TextMeshPro>().text = name;
				newGameObject.transform.position = coordinate;
				districtNames.Add(newGameObject.GetComponent<District>());
				//Debug.Log(index);

				yield return null;
			}
		}
		yield return null;

		var neighbourhoodsRequest = UnityWebRequest.Get($"{Config.activeConfiguration.webserverRootPath}{neighbourhoodsFilepath}");
		yield return neighbourhoodsRequest.SendWebRequest();
		index = 0;
		if (districtsRequest.result == UnityWebRequest.Result.Success)
		{
			string neighbourhoodsText = neighbourhoodsRequest.downloadHandler.text;
			int nextIndex;
			int nameEndIndex;
			bool keepGoing = true;
			while (keepGoing)
			{
				nextIndex = neighbourhoodsText.IndexOf(latstring, index) + latstring.Length;
				if (nextIndex < index)
				{
					keepGoing = false; break;
				}
				index = nextIndex;
				if (index == -1) { keepGoing = false; break; }
				double lat = StringManipulation.ParseNextDouble(neighbourhoodsText, ',', index, out index);
				index = neighbourhoodsText.IndexOf(lonstring, index) + lonstring.Length;
				if (index == -1) { keepGoing = false; break; }
				double lon = StringManipulation.ParseNextDouble(neighbourhoodsText, ',', index, out index);
				index = neighbourhoodsText.IndexOf(namestring, index) + namestring.Length + 1;
				if (index == -1) { keepGoing = false; break; }
				nameEndIndex = neighbourhoodsText.IndexOf('"', index);
				string name = neighbourhoodsText.Substring(index, nameEndIndex - index);
				index = nameEndIndex;
				if (index < 0) { keepGoing = false; break; }
				Vector3 coordinate = CoordConvert.WGS84toUnity(lon, lat);
				coordinate.y = Config.activeConfiguration.zeroGroundLevelY + 2.0f;

				GameObject go = Instantiate(districtMarker, transform);
				//go.transform.position = coordinate;
				go.GetComponentInChildren<TMPro.TextMeshPro>().text = name;

				go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				go.transform.position = coordinate;
				neighbourhoodNames.Add(go.GetComponent<District>());

				yield return null;
			}
		}
		yield return null;
	}

	void Update()
	{
		DisplayAreasBasedOnCameraTransform();
	}

	private void DisplayAreasBasedOnCameraTransform()
	{
		Transform cameraTransform = CameraModeChanger.Instance.ActiveCamera.transform;

		Vector3 cameraPosition = cameraTransform.position;
		Quaternion cameraRotation = cameraTransform.rotation;

		float camheight = cameraTransform.position.y;
		float camAngle = cameraTransform.localRotation.eulerAngles.x;
		float maxDistance = 0;

		if (camAngle > 45f) // looking down;
		{
			maxDistance = 2 * 2000;
		}
		else
		{
			maxDistance = 5 * camheight;
		}
		if (camheight < hideDistance)
		{
			maxDistance = 0;
		}
		Transform itemTransform;
		float characterSize = (1 + Mathf.Max(camheight / 500, 0)) * 200;

		foreach (District item in districtNames)
		{
			itemTransform = item.transform;
			if ((itemTransform.position - cameraPosition).magnitude < maxDistance)
			{
				item.gameObject.SetActive(true);
			}
			else
			{
				item.gameObject.SetActive(false);
			}
			itemTransform.rotation = cameraRotation;
			item.textMeshPro.fontSize = characterSize;
		}

		foreach (District item in neighbourhoodNames)
		{
			itemTransform = item.transform;
			if ((itemTransform.position - cameraPosition).magnitude < 0.5 * maxDistance)
			{
				item.gameObject.SetActive(true);
			}
			else
			{
				item.gameObject.SetActive(false);
			}

			itemTransform.rotation = cameraRotation;
			item.textMeshPro.fontSize = characterSize;
		}
	}
}
