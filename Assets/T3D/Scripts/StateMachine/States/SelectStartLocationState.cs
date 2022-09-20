using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
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
    [SerializeField]
    private WorldPointFollower instructionsTagPrefab;
    private WorldPointFollower instructionsTag;
    private float orthographicCameraDefaultSize;

    [SerializeField]
    private GameObject visibilityPanel;

    protected override void Awake()
    {
        base.Awake();
        building = RestrictionChecker.ActiveBuilding;
    }

    private void Update()
    {
        CalculateAndApplyPlaceLocation();

        if (!ServiceLocator.GetService<Selector>().HoveringInterface() && Input.GetMouseButtonDown(0))
        {
            StepEndedByUser();
        }

        if (orthographicCameraDefaultSize == 0)
        {
            orthographicCameraDefaultSize = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.orthographicSize;
        }

        var tagScaleFactor = orthographicCameraDefaultSize / ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.orthographicSize;
        instructionsTag.transform.localScale = Vector3.one * tagScaleFactor;
    }

    private void CalculateAndApplyPlaceLocation()
    {
        var groundPlane = new Plane(transform.up, -building.GroundLevel);
        var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
        var cast = groundPlane.Raycast(ray, out float enter);
        if (cast)
        {
            var offset = RestrictionChecker.ActiveUitbouw.CenterPoint - RestrictionChecker.ActiveUitbouw.transform.position;
            placeLocation = ray.origin + (ray.direction * enter) - offset;
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetPosition(placeLocation);
            instructionsTag.AlignWithWorldPosition(RestrictionChecker.ActiveUitbouw.CenterPoint);
        }
    }

    public override int GetDesiredStateIndex()
    {
        desiredNextStateIndex = ServiceLocator.GetService<T3DInit>().HTMLData.HasFile ? 0 : 1;
        return desiredNextStateIndex;
    }

    public override void StateEnteredAction()
    {
        //if (!RestrictionChecker.ActiveUitbouw)
        //{
        ServiceLocator.GetService<MetadataLoader>().PlaatsUitbouw(placeLocation);
        //}

        ServiceLocator.GetService<CameraModeChanger>().SetCameraMode(CameraMode.TopDown);
        building.transform.position += Vector3.up * 0.001f; //fix z-fighting in orthographic mode

        //ServiceLocator.GetService<MetadataLoader>().EnableActiveuitbouw(true);
        //DisableUitbouwToggle.Instance.SetIsOnWithoutNotify(true);
        //RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(true);
        //RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(false); //disable movement and measuring lines
        //RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(false); //disable rotation
        //RestrictionChecker.ActiveUitbouw.EnableGizmo(false);
        RestrictionChecker.ActiveUitbouw.GetComponent<Outline>().enabled = true;

        //RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(false); //disable uitbouw that was already placed, but preserve any boundary features that were added
        instructionsTag = ServiceLocator.GetService<CoordinateNumbers>().CreateGenericWorldPointFollower(instructionsTagPrefab);

        foreach (var uiToggle in visibilityPanel.GetComponentsInChildren<UIToggle>(true))
        {
            if (uiToggle as DisableMainBuildingToggle)
            {
                uiToggle.SetIsOn(true);
                uiToggle.SetVisible(false);
                continue;
            }

            if (uiToggle as DisableUitbouwToggle)
            {
                uiToggle.SetIsOn(true);
                uiToggle.SetVisible(false);
                continue;
            }
        }
    }

    public override void StateCompletedAction()
    {
        ServiceLocator.GetService<CameraModeChanger>().SetCameraMode(CameraMode.GodView);
        building.transform.position -= Vector3.up * 0.001f; //reset position 
        building.SelectedWall.AllowSelection = false;
        RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(true);
        RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(true);
        RestrictionChecker.ActiveUitbouw.GetComponent<Outline>().enabled = false;
        Destroy(instructionsTag.gameObject);

        foreach (var uiToggle in visibilityPanel.GetComponentsInChildren<UIToggle>(true))
        {
            print(uiToggle);
            if (uiToggle as DisableMainBuildingToggle)
            {
                var uploadedUitbouw = ServiceLocator.GetService<T3DInit>().HTMLData.HasFile;
                var drawChange = ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel;
                uiToggle.SetVisible(uploadedUitbouw && drawChange);
                continue;
            }

            if (uiToggle as DisableUitbouwToggle)
            {
                var drawChange = ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel;
                uiToggle.SetVisible(drawChange);
                continue;
            }
        }
    }
}
