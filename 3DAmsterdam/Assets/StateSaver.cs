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
        activeStateIndexKey = GetType().Namespace + GetType().ToString() + ".activeStateIndex";
        ActiveStateIndex = new SaveableInt(activeStateIndexKey, SessionSaver.LoadPreviousSession);
    }

    private void OnEnable()
    {
        State.ActiveStateChanged += State_ActiveStateChanged;
    }

    private void OnDisable()
    {
        State.ActiveStateChanged -= State_ActiveStateChanged;
    }

    private void State_ActiveStateChanged(State newState)
    {
        print("saving new state: " + GetStateIndex(newState));
        ActiveStateIndex.SetValue(GetStateIndex(newState));
    }

    private void Start()
    {
        states = GetComponentsInChildren<State>(true);
        print(states.Length);
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
