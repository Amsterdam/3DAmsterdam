using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class PermitNeededState : State
{
    [SerializeField]
    private GameObject noPermitNeededPanel;
    [SerializeField]
    private GameObject permitNeededPanel;

    private string permissionToUseDesignKey;
    private SaveableBool permissionToUseDesign;

    private bool conformsToAllRestrictions;

    [SerializeField]
    private Toggle permissionToggle;

    protected override void Awake()
    {
        base.Awake();
        permissionToUseDesignKey = GetType() + ".PermissionToUseDesign";
        permissionToUseDesign = new SaveableBool(permissionToUseDesignKey);
    }

    public override void StateEnteredAction()
    {
        //check if restrictions fail
        //var failedRestrictions = RestrictionChecker.NonConformingRestrictions(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);
        conformsToAllRestrictions = RestrictionChecker.ConformsToAllRestrictions(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);

        noPermitNeededPanel.SetActive(conformsToAllRestrictions);
        permitNeededPanel.SetActive(!conformsToAllRestrictions);
    }

    public override void StateCompletedAction()
    {
        if (conformsToAllRestrictions)
        {
            permissionToUseDesign.SetValue(permissionToggle.isOn);
        }
        else
        {
            permissionToUseDesign.SetValue(true);
        }
    }

    public override int GetDesiredStateIndex()
    {
        if (conformsToAllRestrictions)
        {
            return -1; //no next step
        }
        else
        {
            return 0; //metadata/submission step
        }
    }
}
