using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateSaverSaveDataContainer : SaveDataContainer
{
    public int ActiveStateIndex;
}

public class StateSaver : MonoBehaviour
{
    public State[] states;
    public int ActiveStateIndex => saveData.ActiveStateIndex;
    private StateSaverSaveDataContainer saveData;

    private void Awake()
    {
        saveData = new StateSaverSaveDataContainer();
    }

    private void OnEnable()
    {
        State.ActiveStateChangedByUser += State_ActiveStateChanged;
    }

    private void OnDisable()
    {
        State.ActiveStateChangedByUser -= State_ActiveStateChanged;
    }

    private void State_ActiveStateChanged(State newState)
    {
        saveData.ActiveStateIndex = GetStateIndex(newState);
    }

    private void Start()
    {
        states = GetComponentsInChildren<State>(true);
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
