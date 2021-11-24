using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class WallSelectionState : State
{
    public override int GetDesiredStateIndex()
    {
        if (MetadataLoader.Instance == null)
            desiredNextStateIndex = 0;
        else
            desiredNextStateIndex = MetadataLoader.Instance.UploadedModel ? 0 : 1;
        return desiredNextStateIndex;
    }

    public override void StateCompletedAction()
    {
        GetComponent<WallSelectionUI>().CompleteStep();
    }
}
