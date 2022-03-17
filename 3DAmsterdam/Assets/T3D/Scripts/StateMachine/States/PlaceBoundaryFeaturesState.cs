using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using UnityEngine;

public class PlaceBoundaryFeaturesStateSaveDataContainer : SaveDataContainer
{
    public int AmountOfPlacedFeatues;
}

public class PlaceBoundaryFeaturesState : State
{
    private PlaceBoundaryFeaturesStateSaveDataContainer saveData;
    public static List<BoundaryFeature> SavedBoundaryFeatures = new List<BoundaryFeature>();

    protected override void Awake()
    {
        base.Awake();
        saveData = new PlaceBoundaryFeaturesStateSaveDataContainer();
    }

    public override void StateLoadedAction()
    {
        //after loading the amount of saved features, this number needs to be reset so it can be reused in this session.
        //var amountOfSavedFeatures = saveData.AmountOfPlacedFeatues;
        saveData.AmountOfPlacedFeatues = 0;

        if (!SessionSaver.LoadPreviousSession)
            return;

        LoadSavedBoundaryFeatures();
    }

    private void LoadSavedBoundaryFeatures()
    {
        var availableComponents = GetComponentsInChildren<SelectComponent>(); //list of boundary feature types in the library
        var boundaryFeatureSaveDataNode = SessionSaver.GetJSONNodeOfType(typeof(BoundaryFeatureSaveData).ToString()); //get the saved data

        foreach (var node in boundaryFeatureSaveDataNode)
        {
            var key = node.Key;
            var data = node.Value;

            var prefabName = data["PrefabName"];
            var selectedComponent = availableComponents.FirstOrDefault(comp => comp.ComponentObject.name == prefabName); // find matching prefab in the library

            if (selectedComponent == null)
            {
                Debug.LogError("Saved component: " + prefabName + " not available in component library.");
                continue;
            }

            var placedBoundaryFeature = Instantiate(selectedComponent.ComponentObject);
            AddBoundaryFeatureToSaveData(placedBoundaryFeature, prefabName);
            placedBoundaryFeature.LoadData();
        }
    }

    public override void StateEnteredAction()
    {
        GetComponent<BoundaryFeatureEditHandler>().SetAllowBoundaryFeatureEditing(true);
    }

    public override void StateCompletedAction()
    {
        GetComponent<BoundaryFeatureEditHandler>().SetAllowBoundaryFeatureEditing(false);
    }

    // called when user drags an item to the uitbouw
    public void AddBoundaryFeatureToSaveData(BoundaryFeature feature, string prefabName)
    {
        saveData.AmountOfPlacedFeatues++;

        int id = saveData.AmountOfPlacedFeatues;
        feature.InitializeSaveData(id, prefabName);
        SavedBoundaryFeatures.Add(feature);
    }

    public void RemoveBoundaryFeatureFromSaveData(BoundaryFeature feature)
    {
        //the ids may need to be adjusted to maintain an increment of 1
        var lastBf = SavedBoundaryFeatures[SavedBoundaryFeatures.Count - 1];
        var deletedID = feature.Id;

        //set the id of the last boundary feature to the id of the feature to be deleted to maintain an increment of 1
        lastBf.SaveData.SetId(deletedID);
        //delete the saveData of the deleted feature.
        feature.SaveData.DeleteSaveData();
        // delete the bf from the list
        SavedBoundaryFeatures.Remove(feature);

        //sort list so that this function will not result in duplicate ids the next time it is called
        SavedBoundaryFeatures = SavedBoundaryFeatures.OrderBy(bf => bf.Id).ToList();
        //decrement amount
        saveData.AmountOfPlacedFeatues--;

        return;
    }
}
