using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class PlaceUitbouwState : State
{
    private UitbouwMovement uitbouwMovement;
    private UitbouwRotation uitbouwRotation;

    protected override void Awake()
    {
        base.Awake();
        uitbouwMovement = RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>();
        uitbouwRotation = RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>();
    }

    public override void StateEnteredAction()
    {
        base.StateEnteredAction();
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
}
