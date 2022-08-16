using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Netherlands3D.T3D.Uitbouw;

public class StateSaverSaveDataContainer : SaveDataContainer
{
    public int ActiveStateIndex;
}

public class StateSaver : MonoBehaviour, IUniqueService
{
    public State[] states;
    public int ActiveStateIndex => saveData.ActiveStateIndex;
    private StateSaverSaveDataContainer saveData;


    private void Awake()
    {
        saveData = new StateSaverSaveDataContainer();
        states = GetComponentsInChildren<State>(true);
    }

    private void OnEnable()
    {
        State.ActiveStateChangedByUser += State_ActiveStateChanged;
    }

    private void OnDisable()
    {
        State.ActiveStateChangedByUser -= State_ActiveStateChanged;
        //ServiceLocator.GetService<MetadataLoader>().BuildingMetaDataLoaded -= BuildingMetaDataLoaded;
    }

    private void Start() // in start to avoid timing issues
    {
        GoToFirstState();
    }

    private void State_ActiveStateChanged(State newState)
    {
        saveData.ActiveStateIndex = GetStateIndex(newState);
    }

    private void GoToFirstState()
    {
        var firstState = states.FirstOrDefault(s => s.IsFirstState);
        firstState.EnterStartState();
        //firstState.gameObject.SetActive(true);
    }

    public int GetStateIndex(State state)
    {
        return Array.IndexOf(states, state);
    }

    public State GetState(int index)
    {
        return states[index];
    }
}
