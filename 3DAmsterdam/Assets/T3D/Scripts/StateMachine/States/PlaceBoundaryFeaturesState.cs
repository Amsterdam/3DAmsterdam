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
        var amountOfSavedFeatures = saveData.AmountOfPlacedFeatues;
        saveData.AmountOfPlacedFeatues = 0;

        print("loading " + saveData.AmountOfPlacedFeatues + "features");

        if (!SessionSaver.LoadPreviousSession)
            return;

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
            AddBoundaryFeatureToSaveData(placedBoundaryFeature, prefabName);
            placedBoundaryFeature.LoadData();
        }
        //for (int i = 1; i <= amountOfSavedFeatures; i++)
        //{
        //    LoadBoundaryFeature(i);
        //}
    }

    public override void StateEnteredAction()
    {
        GetComponent<BoundaryFeatureEditHandler>().SetAllowBoundaryFeatureEditing(true);
    }

    public override void StateCompletedAction()
    {
        GetComponent<BoundaryFeatureEditHandler>().SetAllowBoundaryFeatureEditing(false);
    }

    //private void LoadBoundaryFeature(int id)
    //{
    //    BoundaryFeature componentObject = null;

    //    var components = GetComponentsInChildren<SelectComponent>();
    //    //var saveData = new BoundaryFeatureSaveData_OLD(id);

    //    foreach (var component in components)
    //    {
    //        if (component.ComponentObject.name == saveData.PrefabName.Value)
    //        {
    //            componentObject = component.ComponentObject;
    //        }
    //    }

    //    if (componentObject == null) return;

    //    //var wall = RestrictionChecker.ActiveUitbouw.GetWall();
    //    var placedBoundaryFeature = Instantiate(componentObject, saveData.Position.Value, saveData.Rotation.Value);
    //    placedBoundaryFeature.LoadData(id);
    //    AddBoundaryFeatureToSaveData(componentObject.name, placedBoundaryFeature);
    //}

    // called when user drags an item to the uitbouw
    public void AddBoundaryFeatureToSaveData(BoundaryFeature feature, string prefabName)
    {
        saveData.AmountOfPlacedFeatues++;

        int id = saveData.AmountOfPlacedFeatues;
        feature.InitializeSaveData(id, prefabName);
        SavedBoundaryFeatures.Add(feature);
        //feature.SetId(id);
        //feature.PrefabName = prefabName;
        //feature.InitializeSaveData(id); //todo: removed
        //feature.UpdateMetadata(id, prefabName);//todo: removed
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

        //if the last feautre is not the one being removed, the ids need to be changed
        if (lastBf != feature)
        {
            //remove old keys from the save data
            //lastBf.SaveData.Delete();
            //set the last saved id to the removed id this way the ids remain incremented by 1.
            lastBf.SaveData.SetId(feature.Id);
            //now we can delete the save data of the bf that is being deleted
            feature.SaveData.DeleteSaveData();
            //lastBf.UpdateMetadata(feature.Id, lastBf.PrefabName);
        }
        else //if the last bf is being removed, just delete those keys
        {
            lastBf.SaveData.DeleteSaveData();
        }

        // delete the bf from the list
        SavedBoundaryFeatures.Remove(feature);

        //sort list so that this function will not result in duplicate ids the next time it is called
        SavedBoundaryFeatures = SavedBoundaryFeatures.OrderBy(bf => bf.Id).ToList();
        //decrement amount
        saveData.AmountOfPlacedFeatues--;
    }
}
