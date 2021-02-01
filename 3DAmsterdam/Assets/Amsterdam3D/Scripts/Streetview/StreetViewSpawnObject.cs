using Amsterdam3D.Interface;
using BruTile.Wms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Amsterdam3D.CameraMotion;


public class StreetViewSpawnObject : MonoBehaviour
{
	[SerializeField]
	private GameObject streetViewPrefab;

	[SerializeField]
	private Transform UIParent;

	private int currentIndex = 1;

	private bool canClick = false;

	public float offset = 1.8f;

	private GameObject currentObject;
	private FirstPersonLocation currentFirstPersonLocation;
	private WorldPointFollower follower;

	private Pointer pointer;

	private InterfaceLayers layers;

	[SerializeField]
	private LayerMask layerMask;

	private void Awake()
	{
		pointer = FindObjectOfType<Pointer>();
		layers = FindObjectOfType<InterfaceLayers>();
	}
	private void Update()
	{
		if (canClick)
		{
			RaycastHit hit;
			Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 9999, layerMask.value))
			{
				follower.WorldPosition = hit.point + Vector3.up * offset;
				if (Input.GetMouseButtonDown(0))
				{
					canClick = false;
					currentFirstPersonLocation.placed = true;
					currentObject = null;
				}
			}
		}
	}

	public void SpawnFirstPersonPrefab()
	{
		SpawnFirstPersonAtPosition(pointer.WorldPosition, Quaternion.Euler(0, 0, 0), true);
	}

	public void SpawnFirstPersonAtPosition(Vector3 position, Quaternion rotation, bool manualPlace = false)
	{
		currentObject = Instantiate(streetViewPrefab);
		currentObject.name = "Camera punt " + currentIndex;
		currentIndex++;
		layers.AddNewCustomObjectLayer(currentObject, LayerType.CAMERA, true);
		currentObject.transform.SetParent(UIParent, false);
		currentFirstPersonLocation = currentObject.GetComponent<FirstPersonLocation>();
		follower = currentObject.GetComponent<WorldPointFollower>();
		follower.WorldPosition = position;
		if (manualPlace)
		{
			canClick = true;
		}
		else
		{
			currentFirstPersonLocation.placed = true;
			currentFirstPersonLocation.savedRotation = rotation;
			currentObject.gameObject.SetActive(false);
		}
	}

}
