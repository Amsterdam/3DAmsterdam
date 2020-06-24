using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    private GameObject _selectedObject;
    private SelectAndScale script;

    private void Start()
    {
        script = GameObject.Find("UploadGebouwMenu").GetComponent<SelectAndScale>();

    }

    private void Update()
    {
        _selectedObject = script.selectedObject;
    }

    public void Remove()
    {
        Destroy(_selectedObject.gameObject);
    }
}
