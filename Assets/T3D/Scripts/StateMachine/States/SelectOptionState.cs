using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class SelectOptionState : State
{
    [SerializeField]
    private Toggle noModelToggle, drawToggle, uploadedModelToggle, snapToggle, freePlaceToggle, otherBuildingPartToggle;
    [SerializeField]
    private GameObject modelSettingsPanel, uploadPanel, notSupportedPanel;
    [SerializeField]
    private Button nextButton;
    private CityJsonVisualiser visualiser;

    protected override void Awake()
    {
        base.Awake();
        visualiser = ServiceLocator.GetService<CityJsonVisualiser>();
    }

    private void Update()
    {
        modelSettingsPanel.SetActive(!noModelToggle.isOn);
        notSupportedPanel.SetActive(otherBuildingPartToggle.isOn);
        uploadPanel.SetActive(uploadedModelToggle.isOn);
        if (noModelToggle.isOn)
            nextButton.interactable = true;
        else if (uploadedModelToggle.isOn)
            nextButton.interactable = !otherBuildingPartToggle.isOn && visualiser.HasLoaded;
        else
            nextButton.interactable = !otherBuildingPartToggle.isOn;
    }

    public override int GetDesiredStateIndex()
    {
        ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel = !noModelToggle.isOn;
        ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall = snapToggle.isOn;
        ServiceLocator.GetService<T3DInit>().HTMLData.HasFile = uploadedModelToggle.isOn;

        if (noModelToggle.isOn)
        {
            return 0;
        }
        else if (snapToggle.isOn)
            return 1;
        else
            return 2;
    }

    protected override void LoadSavedState()
    {
        LoadModelToggles();

        uploadedModelToggle.isOn = ServiceLocator.GetService<T3DInit>().HTMLData.HasFile;

        StateLoadedAction();
        var stateSaver = GetComponentInParent<StateSaver>();
        var savedState = stateSaver.GetState(stateSaver.ActiveStateIndex);
        if (ActiveState != savedState)
        {
            if (uploadedModelToggle.isOn)
                LoadModel();
            StartCoroutine(LoadModelAndGoToNextState());
        }
    }

    private void LoadModelToggles()
    {
        if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel && ServiceLocator.GetService<T3DInit>().HTMLData.HasFile)
        {
            uploadedModelToggle.isOn = true;
            LoadSnapToggles();
        }
        else if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel && !ServiceLocator.GetService<T3DInit>().HTMLData.HasFile)
        {
            drawToggle.isOn = true;
            LoadSnapToggles();
        }
        else
        {
            noModelToggle.isOn = true;
        }
    }

    private void LoadSnapToggles()
    {
        if (ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall)
            snapToggle.isOn = true;
        else
            freePlaceToggle.isOn = true;
    }

    //called by button in inspector
    public void LoadModel()
    {
        visualiser.VisualizeCityJson();
    }

    public IEnumerator LoadModelAndGoToNextState()
    {
        if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel && ServiceLocator.GetService<T3DInit>().HTMLData.HasFile)
        {
            var visualizer = ServiceLocator.GetService<CityJsonVisualiser>();
            visualizer.VisualizeCityJson();
            yield return new WaitUntil(() =>
                visualizer.HasLoaded
            );
        }
        EndState();
    }

    public override void StateEnteredAction()
    {
        base.StateEnteredAction();
        RestrictionChecker.ActiveBuilding.SelectedWall.gameObject.SetActive(false);
        ServiceLocator.GetService<MetadataLoader>().EnableActiveuitbouw(false);
    }
}
