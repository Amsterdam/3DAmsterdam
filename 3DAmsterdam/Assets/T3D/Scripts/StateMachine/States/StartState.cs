using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class StartState : State
{
    public override int GetDesiredStateIndex()
    {
        if (T3DInit.HTMLData == null)
            return 0;

        return T3DInit.HTMLData.SnapToWall ? 0 : 1;
    }

    public override void StateEnteredAction()
    {
        StartCoroutine(WaitForDataLoadingToComplete());
    }

    private IEnumerator WaitForDataLoadingToComplete()
    {
        //wait until all data is loaded to avoid timing issues in later steps
        yield return new WaitUntil(() =>
            SessionSaver.HasLoaded &&
            RestrictionChecker.ActiveBuilding.BuildingDataIsProcessed &&
            RestrictionChecker.ActivePerceel.IsLoaded
        );

        GoToNextState();
    }

    private void GoToNextState()
    {
        if (StateSaver.Instance.ActiveStateIndex > 0)
            EndState();
        else
            StepEndedByUser(); //use By User function to ensure state Index in incremented;
    }

    //public override void StateEnteredAction()
    //{
    //    var saver = GetComponentInParent<StateSaver>();
    //    if (saver.ActiveStateIndex == saver.GetStateIndex(this))
    //        EndState();
    //}
}
