using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class WallSelectionState : State
{
    public override int GetDesiredStateIndex()
    {
        if (MetadataLoader.Instance == null)
            desiredStateIndex = 0;
        else
            desiredStateIndex = MetadataLoader.Instance.UploadedModel ? 0 : 1;
        return desiredStateIndex;
    }
}
