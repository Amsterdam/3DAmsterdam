using Amsterdam3D.CameraMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstPersonObject : MonoBehaviour, IPointerClickHandler
{

	private CameraModeChanger manager;

	public bool placed = false;
	private WorldPointFollower follower;

	[HideInInspector]
	public Quaternion savedRotation = Quaternion.Euler(Vector3.zero);

	private void Awake()
	{
		manager = CameraModeChanger.Instance;
		manager.OnGodViewModeEvent += EnableObject;
		manager.OnFirstPersonModeEvent += DisableObject;
		follower = GetComponent<WorldPointFollower>();
	}

	private void EnableObject()
	{
		gameObject.SetActive(true);
	}

	private void DisableObject()
	{
		gameObject.SetActive(false);
	}
	private void OnMouseDown()
	{
		if (placed)
		{
			manager.FirstPersonMode(follower.WorldPosition, savedRotation);
			gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		manager.OnGodViewModeEvent -= EnableObject;
		manager.OnFirstPersonModeEvent -= DisableObject;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (placed)
		{
			manager.FirstPersonMode(follower.WorldPosition, savedRotation);
			gameObject.SetActive(false);
		}
	}
}
