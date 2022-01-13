using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using UnityEngine;

public class PlaceBoundaryFeaturesState : State
{
    string amountOfPlacedFeatuesKey;
    SaveableInt amountOfPlacedFeatues;
    //List<int> ids = new List<int>();
    //SaveableIntArray ids; //save the actual placed ids, the id increments every time a new bf is placed, but does not decrement when a bf is deleted to avoid having to change all ids to account for the missing id
    List<BoundaryFeature> savedBoundaryFeatures = new List<BoundaryFeature>();

    protected override void Awake()
    {
        base.Awake();
        amountOfPlacedFeatuesKey = GetType().ToString() + ".amountOfPlacedFeatues";
        amountOfPlacedFeatues = new SaveableInt(amountOfPlacedFeatuesKey);
    }

    public override void StateLoadedAction()
    {
        //after loading the amount of saved features, this number needs to be reset so it can be reused in this session.
        var amountOfSavedFeatures = amountOfPlacedFeatues.Value;
        amountOfPlacedFeatues.SetValue(0);

        for (int i = 1; i <= amountOfSavedFeatures; i++)
        {
            LoadBoundaryFeature(i);
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

    private void LoadBoundaryFeature(int id)
    {
        BoundaryFeature componentObject = null;

        var components = GetComponentsInChildren<SelectComponent>();
        var saveData = new BoundaryFeatureSaveData(id);

        foreach (var component in components)
        {
            if (component.ComponentObject.name == saveData.PrefabName.Value)
            {
                componentObject = component.ComponentObject;
            }
        }

        //var wall = RestrictionChecker.ActiveUitbouw.GetWall();
        var placedBoundaryFeature = Instantiate(componentObject, saveData.Position.Value, saveData.Rotation.Value);
        placedBoundaryFeature.LoadData(id);
        AddBoundaryFeatureToSaveData(componentObject.name, placedBoundaryFeature);
    }

    public void AddBoundaryFeatureToSaveData(string prefabName, BoundaryFeature feature)
    {
        amountOfPlacedFeatues.SetValue(amountOfPlacedFeatues.Value + 1);

        int id = amountOfPlacedFeatues.Value;
        //feature.SetId(id);
        //feature.PrefabName = prefabName;
        feature.InitializeSaveData(id);
        feature.UpdateMetadata(id, prefabName);
        savedBoundaryFeatures.Add(feature);
    }

    public void RemoveBoundaryFeatureFromSaveData(BoundaryFeature feature)
    {
        //the ids may need to be adjusted to maintain an increment of 1
        var lastBf = savedBoundaryFeatures[savedBoundaryFeatures.Count - 1];

        //if the last feautre is not the one being removed, the ids need to be changed
        if (lastBf != feature)
        {
            //remove old keys from the save data
            lastBf.SaveData.DeleteKeys();
            //set the last saved id to the removed id this way the ids remain incremented by 1.
            lastBf.UpdateMetadata(feature.Id, lastBf.PrefabName);
        }
        else //if the last bf is being removed, just delete those keys
        {
            lastBf.SaveData.DeleteKeys();
        }

        // delete the bf from the list
        savedBoundaryFeatures.Remove(feature);

        //sort list so that this function will not result in duplicate ids the next time it is called
        savedBoundaryFeatures = savedBoundaryFeatures.OrderBy(bf => bf.Id).ToList();
        //decrement amount
        amountOfPlacedFeatues.SetValue(amountOfPlacedFeatues.Value - 1);

    }
}
