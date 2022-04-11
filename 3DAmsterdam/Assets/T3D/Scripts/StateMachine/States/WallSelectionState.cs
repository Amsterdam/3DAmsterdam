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

    protected override void Awake()
    {
        base.Awake();
        building = RestrictionChecker.ActiveBuilding;
        building.SelectedWall.AllowSelection = true;

        nextButton.interactable = false;
    }

    private void Update()
    {
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

    private bool LocationIsSelected()
    {
        return building.SelectedWall.WallIsSelected;
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
        building.SelectedWall.AllowSelection = true;
        building.SelectedWall.WallChanged = false;

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
        return building.SelectedWall.transform.position + building.SelectedWall.WallMesh.bounds.center;
    }
}
