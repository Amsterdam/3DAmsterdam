using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using UnityEngine;

public class PlaceBoundaryFeaturesState : State
{
    public static List<BoundaryFeature> SavedBoundaryFeatures = new List<BoundaryFeature>();
    public int AmountOfPlacedFeatues => SavedBoundaryFeatures.Count;

    protected override void Awake()
    {
        base.Awake();
        SavedBoundaryFeatures = new List<BoundaryFeature>(); //ensure the static list is emptied whenever the scene is reset
    }

    public override void StateLoadedAction()
    {
        if (SessionSaver.LoadPreviousSession)
            LoadSavedFeatures();
    }

    private void LoadSavedFeatures()
    {
        var availableComponents = GetComponentsInChildren<SelectComponent>();

        var boundaryFeatureSaveDataNode = SessionSaver.GetJSONNodeOfType(typeof(BoundaryFeatureSaveData).ToString());

        foreach (var node in boundaryFeatureSaveDataNode)
        {
            var key = node.Key;
            var data = node.Value;

            var prefabName = data["PrefabName"];
            var selectedComponent = availableComponents.FirstOrDefault(comp => comp.ComponentObject.name == prefabName);

            if (selectedComponent == null)
            {
                Debug.LogError("Saved component: " + prefabName + " not available in component library.");
                continue;
            }

            var placedBoundaryFeature = Instantiate(selectedComponent.ComponentObject/*savedPosition, savedRotation*/);
            //placedBoundaryFeature.LoadData(int.Parse(key), prefabName);
            AddBoundaryFeatureToSaveData(placedBoundaryFeature, prefabName, key);
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
    public void AddNewBoundaryFeatureToSaveData(BoundaryFeature feature, string prefabName)
    {
        int id = AmountOfPlacedFeatues;
        AddBoundaryFeatureToSaveData(feature, prefabName, id.ToString());
    }

    //called to load data, since the keys are already known
    private void AddBoundaryFeatureToSaveData(BoundaryFeature feature, string prefabName, string id)
    {
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

        return;
    }
}
