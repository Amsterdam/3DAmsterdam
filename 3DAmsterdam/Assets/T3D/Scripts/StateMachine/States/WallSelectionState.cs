using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class WallSelectionState : State
{
    public override int GetDesiredStateIndex()
    {
        if (T3DInit.Instance == null)
            desiredNextStateIndex = 0;
        else
            desiredNextStateIndex = T3DInit.Instance.UploadedModel ? 0 : 1;
        return desiredNextStateIndex;
    }

    public override void StateCompletedAction()
    {
        GetComponent<WallSelectionUI>().CompleteStep();
    }
}
