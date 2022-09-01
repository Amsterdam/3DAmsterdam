using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class PlaceUitbouwState : State
{
    private BuildingMeshGenerator building;

    private UitbouwMovement uitbouwMovement;
    private UitbouwRotation uitbouwRotation;

    protected override void Awake()
    {
        base.Awake();
        building = RestrictionChecker.ActiveBuilding;
    }

    public override void StateEnteredAction()
    {
        base.StateEnteredAction();
        CreateOrEnableUitbouw(GetSpawnPosition());

        uitbouwMovement = RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>();
        uitbouwRotation = RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>();

        DisableUitbouwToggle.Instance.SetIsOnWithoutNotify(true);
        RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(true);
        RestrictionChecker.ActiveUitbouw.EnableGizmo(true);
        if (!uitbouwMovement.AllowDrag)
        {
            uitbouwMovement.SetAllowMovement(true);
            uitbouwRotation.SetAllowRotation(true);
        }
    }

    public override void StateCompletedAction()
    {
        base.StateCompletedAction();
        RestrictionChecker.ActiveUitbouw.EnableGizmo(false);
        RestrictionChecker.ActiveUitbouw.UpdateDimensions(); // force update the dimensions since these may not have been set yet when reloading and leaving this state
        uitbouwMovement.SetAllowMovement(false);
        uitbouwRotation.SetAllowRotation(false);
    }

    private Vector3 GetSpawnPosition()
    {
        if (ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall)
            return building.SelectedWall.transform.position + building.SelectedWall.WallMesh.bounds.center;
        else
            return RestrictionChecker.ActiveUitbouw.transform.position;
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
                RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetPosition(location);
            }
        }
        else
        {
            //create uitbouw since there was no uitbouw previously placed
            ServiceLocator.GetService<MetadataLoader>().PlaatsUitbouw(location);
        }
    }
}
