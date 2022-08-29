using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class DisableUitbouwToggle : UIToggle
{
    public static DisableUitbouwToggle Instance;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    protected override void ToggleAction(bool active)
    {
        if (RestrictionChecker.ActiveUitbouw)
        {
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(active); //disable movement and measuring lines
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(active); //disable rotation
            RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(active); //disable uitbouw that was already placed, but preserve any boundary features that were added
            RestrictionChecker.ActiveUitbouw.EnableGizmo(active && (State.ActiveState.GetType() == typeof(PlaceUitbouwState)));
        }
    }

    public void SetIsOnWithoutNotify(bool value)
    {
        toggle.SetIsOnWithoutNotify(value);
    }
}
