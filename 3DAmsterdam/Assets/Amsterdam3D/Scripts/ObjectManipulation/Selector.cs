using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
	[SerializeField]
	private OutlineObject outline;
	public static Selector Instance = null;

	public List<OutlineObject> selectedObjects;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		selectedObjects = new List<OutlineObject>();
	}

	/// <summary>
	/// Add an outline object to the target gameobject
	/// </summary>
	/// <param name="gameObject"></param>
	public void HighlightObject(GameObject gameObject)
	{
		ClearHighlights();
		selectedObjects.Add(Instantiate(outline, gameObject.transform));
	}

	/// <summary>
	/// Destroys all the current outlines
	/// </summary>
	private void ClearHighlights(){
		foreach(var outlinedObject in selectedObjects)
		{
			if(outlinedObject != null)
				Destroy(outlinedObject.gameObject);
		}
		selectedObjects.Clear(); //May allow multiselect in future features
	}
}
