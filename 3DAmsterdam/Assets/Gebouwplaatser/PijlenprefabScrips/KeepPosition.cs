using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepPosition : MonoBehaviour {

    Vector3 position;

	void Start () {
        position = transform.position;
    }

    void Update () {
        if (GameObject.Find("Ry").GetComponent<RotateY>().isRotating) transform.position = position;
	}
}
