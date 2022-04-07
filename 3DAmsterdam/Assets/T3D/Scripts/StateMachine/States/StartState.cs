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
            EndState();
    }

    private void BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
    {
        EndState();
    }

    //public override void StateEnteredAction()
    //{
    //    var saver = GetComponentInParent<StateSaver>();
    //    if (saver.ActiveStateIndex == saver.GetStateIndex(this))
    //        EndState();
    //}
}
