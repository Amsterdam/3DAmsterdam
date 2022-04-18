using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class StartState : State
{
    public override int GetDesiredStateIndex()
    {
        if (ServiceLocator.GetService<T3DInit>().HTMLData == null)
            return 0;

        return ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall ? 0 : 1;
    }

    public override void StateEnteredAction()
    {
        StartCoroutine(WaitForDataLoadingToComplete());
    }

    protected override void LoadSavedState()
    {
        //do not call base function, even when loading a session we need to wait for the data to load correctly.
        //base.LoadSavedState();
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
        if (StateSaver.Instance.ActiveStateIndex != StateSaver.Instance.GetStateIndex(this))
            base.LoadSavedState(); //continue loading states the data once the data is loaded
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
