using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateSaver : MonoBehaviour
{
    public State[] states;
    private string activeStateIndexKey;
    public SaveableInt ActiveStateIndex { get; private set; }

    private void Awake()
    {
        activeStateIndexKey = GetType().ToString() + ".activeStateIndex";
        ActiveStateIndex = new SaveableInt(activeStateIndexKey);
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
        ActiveStateIndex.SetValue(GetStateIndex(newState));
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
