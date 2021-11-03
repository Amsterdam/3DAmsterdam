using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// - Update the selected icon
/// - Do raycast to Uitbouw, when hit, place the component is real size on the selected Uitbouw wall
/// </summary>
public class HandleDragContainer : MonoBehaviour
{
    public GameObject ComponentObject;
    public Image ComponentImage;    
    public int MouseXOffset = 92;
    
    private GameObject PlacedObject;
    public bool isTopComponent;

    private SelectComponent selectComponent;

    private void Awake()
    {
        LibraryComponentSelectedEvent.Subscribe(OnSelect);
    }

    private void OnSelect(object sender, LibraryComponentSelectedEvent.LibraryComponentSelectedEventArgs e)
    {        
        ComponentImage.sprite = e.Image.sprite;
        isTopComponent = e.IsTopComponent;
        ComponentObject = e.ComponentObject;
        selectComponent = e.SelectComponent;

        ComponentObject.transform.localScale = new Vector3(e.ComponentWidth, e.ComponentHeight, 1);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) == false && PlacedObject != null)
        {
            PlacedObject = null;
            selectComponent.SetToggle(false);
            return;
        }

        if (Input.GetMouseButton(0) == false) return;

        RaycastHit hit;
        var screenpoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y + MouseXOffset, 0);        
        Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(screenpoint);

        bool casted = Physics.Raycast(ray, out hit);

        if (hit.transform == null) return;

        var hitname = hit.transform.name.ToLower();
        bool allowed = isTopComponent && hitname == "top" || (!isTopComponent && ( hitname == "left" || hitname == "right" || hitname == "front" ));

        if (casted && allowed )
        {
            var wall = hit.transform.GetComponent<UitbouwMuur>();

            if (PlacedObject == null)
            {                             
                PlacedObject = Instantiate(ComponentObject);                                
            }
            
            PlacedObject.transform.position = hit.point;
            PlacedObject.transform.rotation = hit.transform.rotation;

            ComponentImage.enabled = false;       
            
        }
        else if(PlacedObject != null)
        {
            Destroy(PlacedObject);
            PlacedObject = null;
            ComponentImage.enabled = true;
        }
    }


}
