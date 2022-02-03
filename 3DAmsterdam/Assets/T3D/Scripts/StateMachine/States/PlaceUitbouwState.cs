using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class PlaceUitbouwState : State
{
    private UitbouwMovement uitbouwMovement;

    protected override void Awake()
    {
        base.Awake();
        uitbouwMovement = RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>();
    }
    public override void StateEnteredAction()
    {
        base.StateEnteredAction();
        if (!uitbouwMovement.AllowDrag)
            uitbouwMovement.SetAllowMovement(true);
    }

    public override void StateCompletedAction()
    {
        base.StateCompletedAction();
        uitbouwMovement.SetAllowMovement(false);
    }
}
