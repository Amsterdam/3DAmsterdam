using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class StartState : State
{
    [SerializeField]
    private GameObject visibilityPanel;

    public override int GetDesiredStateIndex()
    {
        return 2;
        if (ServiceLocator.GetService<T3DInit>().HTMLData == null)
            return 0;

        if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel == false) return 2;

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
        //if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel && ServiceLocator.GetService<T3DInit>().HTMLData.HasFile)
        //{
        //    var visualizer = ServiceLocator.GetService<CityJsonVisualiser>();
        //    visualizer.VisualizeCityJson();
        //    yield return new WaitUntil(() =>
        //        visualizer.HasLoaded
        //    );
        //}

        GoToNextState();
    }

    private void GoToNextState()
    {
        var stateSaver = ServiceLocator.GetService<StateSaver>();
        if (stateSaver.ActiveStateIndex != stateSaver.GetStateIndex(this))
            base.LoadSavedState(); //continue loading states the data once the data is loaded
        else
            StepEndedByUser(); //use By User function to ensure state Index in incremented;
    }

    public override void StateCompletedAction()
    {
        base.StateCompletedAction();
        visibilityPanel.SetActive(true);
    }

    //public override void StateEnteredAction()
    //{
    //    var saver = GetComponentInParent<StateSaver>();
    //    if (saver.ActiveStateIndex == saver.GetStateIndex(this))
    //        EndState();
    //}
}
