using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class WallSelectionState : State
{
    private BuildingMeshGenerator building;

    [SerializeField]
    private Button nextButton;
    [SerializeField]
    private Text infoText;

    [SerializeField]
    private string selectedText = "aangegeven gevel";
    [SerializeField]
    private string unselectedText = "selecteer gevel";

    //private bool SnapToWall => T3DInit.HTMLData.SnapToWall;

    //private bool allowSelectPlaceLocation;
    //private Vector3 placeLocation;

    protected override void Awake()
    {
        base.Awake();
        building = RestrictionChecker.ActiveBuilding;
        building.SelectedWall.AllowSelection = true;

        nextButton.interactable = false;
    }

    protected override void Start()
    {
        base.Start();
        //MetadataLoader.Instance.PlaatsUitbouw(placeLocation);
    }

    private void Update()
    {
        //CalculatePlaceLocation();
        ProcessUIState();
    }

    private void ProcessUIState()
    {
        if (LocationIsSelected())
        {
            nextButton.interactable = true;
            infoText.text = selectedText;
        }
        else
        {
            nextButton.interactable = false;
            infoText.text = unselectedText;
        }
    }

    //private void SetAllowSelectPlaceLocation(bool allow)
    //{
    //    allowSelectPlaceLocation = allow;
    //    //placeMarker.gameObject.SetActive(allow);
    //}

    //private void CalculatePlaceLocation()
    //{
    //    var groundPlane = new Plane(transform.up, -building.GroundLevel);
    //    var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
    //    var cast = groundPlane.Raycast(ray, out float enter);
    //    if (cast)
    //    {
    //        placeLocation = ray.origin + (ray.direction * enter);
    //        RestrictionChecker.ActiveUitbouw.transform.position = placeLocation;

    //        if (allowSelectPlaceLocation && Input.GetMouseButtonDown(0))
    //        {
    //            StepEndedByUser();
    //        }
    //    }
    //}

    private bool LocationIsSelected()
    {
        //if (SnapToWall)
            return building.SelectedWall.WallIsSelected;
        //else
        //    return RestrictionChecker.ActiveUitbouw != null;
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
        //if (!SnapToWall)
            //CameraModeChanger.Instance.SetCameraMode(Netherlands3D.Cameras.CameraMode.TopDown);

        building.SelectedWall.AllowSelection = true;
        building.SelectedWall.WallChanged = false;

        //SetAllowSelectPlaceLocation(!SnapToWall);

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
        building.SelectedWall.AllowSelection = false;
        //SetAllowSelectPlaceLocation(false);
        CreateOrEnableUitbouw(GetSpawnPosition());
    }

    private void CreateOrEnableUitbouw(Vector3 location)
    {
        if (RestrictionChecker.ActiveUitbouw)
        {
            //re-enable uitbouw that was previously placed
            RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(true);
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(true); //disable rotation
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(true);
            if (building.SelectedWall.WallChanged)
            {
                RestrictionChecker.ActiveUitbouw.transform.position = location;
            }
        }
        else
        {
            //create uitbouw since there was no uitbouw previously placed
            MetadataLoader.Instance.PlaatsUitbouw(location);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        //if (SnapToWall)
            return building.SelectedWall.transform.position + building.SelectedWall.WallMesh.bounds.center;
        //else
        //    return placeLocation;
    }
}
