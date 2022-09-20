using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class DisableUitbouwToggle : UIToggle
{
    private void Start()
    {
        SetVisible(false);
    }

    protected override void ToggleAction(bool active)
    {
        ServiceLocator.GetService<MetadataLoader>().EnableActiveuitbouw(active);
    }
}
