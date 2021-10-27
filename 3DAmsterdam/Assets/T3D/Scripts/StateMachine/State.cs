using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    [SerializeField]
    protected State[] possibleNextStates;
    protected State previousState;
    [SerializeField]
    private bool isFirstState;
    [SerializeField, ReadOnly]
    protected int desiredStateIndex;

    public static State ActiveState { get; private set; } //allows only 1 state machine throughout the application, maybe change for a dictionary where collections can be defined

    private void Start()
    {
        if (isFirstState)
            ActiveState = this;
    }

    private void OnValidate()
    {
        desiredStateIndex = GetDesiredStateIndex();
    }

    //define which state to chose from the defined possible states
    public virtual int GetDesiredStateIndex()
    {
        return desiredStateIndex;
    }

    public State GetDesiredState()
    {
        int desiredStateIndex = GetDesiredStateIndex();
        if (desiredStateIndex >= 0 && desiredStateIndex < possibleNextStates.Length)
            return possibleNextStates[GetDesiredStateIndex()];
        return null;
    }

    // when the state is entered
    protected virtual void EnterState(State previousState)
    {
        ActiveState = this;
        gameObject.SetActive(true);
        this.previousState = previousState;
    }

    //call to exit the state, optional parameter is to enter a next state
    public virtual void EndState(State nextState)
    {
        if (nextState != null)
        {
            nextState.EnterState(this);
        }
        gameObject.SetActive(false);
    }

    public void EndState()
    {
        EndState(GetDesiredState());
    }

    public void GoToPreviousState()
    {
        if (previousState != null)
        {
            previousState.EnterState(previousState.previousState);
        }
        gameObject.SetActive(false);
    }
}
