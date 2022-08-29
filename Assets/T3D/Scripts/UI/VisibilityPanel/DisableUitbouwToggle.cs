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

    private void Start()
    {
        var drawChange = ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel;
        gameObject.SetActive(drawChange);
        if (!drawChange)
        {
            var sd = transform.parent.GetComponent<RectTransform>().sizeDelta;
            //sizeDelta is a value type so cannot directly assign it
            transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(sd.x, sd.y - GetComponent<RectTransform>().sizeDelta.y);
        }
    }

    protected override void ToggleAction(bool active)
    {
        if (RestrictionChecker.ActiveUitbouw)
        {
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(active && (State.ActiveState.GetType() == typeof(PlaceUitbouwState))); //disable movement and measuring lines
            RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(active && (State.ActiveState.GetType() == typeof(PlaceUitbouwState))); //disable rotation
            RestrictionChecker.ActiveUitbouw.EnableGizmo(active && (State.ActiveState.GetType() == typeof(PlaceUitbouwState)));
            RestrictionChecker.ActiveUitbouw.transform.parent.gameObject.SetActive(active); //disable uitbouw that was already placed, but preserve any boundary features that were added
        }
    }

    public void SetIsOnWithoutNotify(bool value)
    {
        toggle.SetIsOnWithoutNotify(value);
    }
}
