using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class PlaceUitbouwState : State
{
    public override void StateEnteredAction()
    {
        base.StateEnteredAction();
        var test = RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>();
        if (!test.AllowDrag)
            test.SetAllowMovement(true);
    }

    public override void StateCompletedAction()
    {
        base.StateCompletedAction();
        RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(false);
    }
}
