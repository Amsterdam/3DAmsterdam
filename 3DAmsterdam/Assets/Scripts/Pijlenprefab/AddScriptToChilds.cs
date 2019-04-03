using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddScriptToChilds : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.AddComponent<AddPijlenprefab>();
        }        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
