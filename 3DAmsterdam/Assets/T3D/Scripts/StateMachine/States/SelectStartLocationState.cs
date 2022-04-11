using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using Netherlands3D.T3D;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class SelectStartLocationState : State
{
    private BuildingMeshGenerator building;

    [SerializeField]
    private Text infoText;

    [SerializeField]
    private string selectedText = "aangegeven locatie";
    [SerializeField]
    private string unselectedText = "selecteer locatie";

    private Vector3 placeLocation;

    protected override void Awake()
    {
        base.Awake();
        building = RestrictionChecker.ActiveBuilding;
    }

    protected override void Start()
    {
        base.Start();
        MetadataLoader.Instance.PlaatsUitbouw(placeLocation);
    }

    private void Update()
    {
        CalculatePlaceLocation();

        if (!Selector.Instance.HoveringInterface() && Input.GetMouseButtonDown(0))
        {
            StepEndedByUser();
        }
    }

    private void CalculatePlaceLocation()
    {
        var groundPlane = new Plane(transform.up, -building.GroundLevel);
        var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
        var cast = groundPlane.Raycast(ray, out float enter);
        if (cast)
        {
            placeLocation = ray.origin + (ray.direction * enter);
            RestrictionChecker.ActiveUitbouw.transform.position = placeLocation;
        }
    }

    public override int GetDesiredStateIndex()
    {
        if (T3DInit.Instance == null)
            desiredNextStateIndex = 0;
        else
            desiredNextStateIndex = T3DInit.HTMLData.HasFile ? 0 : 1;
        return desiredNextStateIndex;
    }

    public override void StateEnteredAction()
    {
        CameraModeChanger.Instance.SetCameraMode(Netherlands3D.Cameras.CameraMode.TopDown);
        building.transform.position += Vector3.up * 0.001f; //fix z-fighting in orthographic mode

        if (RestrictionChecker.ActiveUitbouw)
        {
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(false); //disable movement and measuring lines
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(false); //disable rotation
            RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(false); //disable uitbouw that was already placed, but preserve any boundary features that were added
        }
    }

    public override void StateCompletedAction()
    {
        CameraModeChanger.Instance.SetCameraMode(Netherlands3D.Cameras.CameraMode.GodView);
        building.transform.position -= Vector3.up * 0.001f; //reset position 
        building.SelectedWall.AllowSelection = false;
        CreateOrEnableUitbouw(placeLocation);
    }

    private void CreateOrEnableUitbouw(Vector3 location)
    {
        if (RestrictionChecker.ActiveUitbouw)
        {
            //re-enable uitbouw that was previously placed
            RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(true);
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(true); //disable rotation
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(true);
            RestrictionChecker.ActiveUitbouw.transform.position = location;
        }
        else
        {
            //create uitbouw since there was no uitbouw previously placed
            MetadataLoader.Instance.PlaatsUitbouw(placeLocation);
        }
    }
}
