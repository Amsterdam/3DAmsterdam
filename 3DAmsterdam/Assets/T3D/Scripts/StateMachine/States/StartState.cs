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

    private void OnEnable()
    {
        MetadataLoader.Instance.BuildingMetaDataLoaded += BuildingMetaDataLoaded;
    }

    private void OnDisable()
    {
        MetadataLoader.Instance.BuildingMetaDataLoaded -= BuildingMetaDataLoaded;
    }

    public override void StateEnteredAction()
    {
        if (RestrictionChecker.ActiveBuilding.BuildingDataIsProcessed)
        {
            GoToNextState();
        }
    }

    private void BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
    {
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
