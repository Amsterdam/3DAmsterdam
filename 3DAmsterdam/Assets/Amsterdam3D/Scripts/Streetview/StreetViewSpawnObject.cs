using Amsterdam3D.Interface;
using BruTile.Wms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


    public class StreetViewSpawnObject:MonoBehaviour
    {
    [SerializeField]
    GameObject streetViewPrefab;

    bool canClick = false;

    public float offset = 1.8f;

    private GameObject currentObject;
    private FirstPersonObject currentObjectComponent;

    private Pointer pointer;

    private InterfaceLayers layers;


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
          Ray ray =   CameraManager.instance.currentCameraComponent.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 9999, 1 << LayerMask.NameToLayer("Terrain"))){
                currentObject.transform.position = hit.point + Vector3.up * offset;
                if (Input.GetMouseButtonDown(0))
                {
                    canClick = false;
                    currentObjectComponent.placed = true;
                    currentObject = null;
                }
            }
        }
    }

    public void SpawnFirstPersonPrefab() 
    {
        currentObject = Instantiate(streetViewPrefab);
        currentObject.transform.position = pointer.WorldPosition;
        layers.AddNewCustomObjectLayer(currentObject, LayerType.CAMERA, true);
        currentObjectComponent = currentObject.GetComponent<FirstPersonObject>();
        canClick = true;

    }

}
