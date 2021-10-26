using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    [SerializeField]
    protected State[] possibleNextStates;
    protected State previousState;
    [SerializeField, ReadOnly]
    protected int desiredStateIndex;

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
        print("entering state " + gameObject.name);
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
}
