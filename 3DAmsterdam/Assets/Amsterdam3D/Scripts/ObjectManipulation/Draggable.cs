using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : ObjectManipulation
{
	[SerializeField]
	private bool snapToGround = false;

	private void OnMouseDrag()
	{
		this.transform.position = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y) - clickOffset;

		if (snapToGround)
			DropDownOnGround();
	}

	private void DropDownOnGround()
	{
		//Raycast down, and move object down towards ray hit.
		throw new NotImplementedException();
	}
}
