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
    protected int desiredNextStateIndex;

    public static State ActiveState { get; private set; } //allows only 1 state machine throughout the application, maybe change for a dictionary where collections can be defined
    public delegate void ActiveStateChangedEventHandler(State newState);
    public static event ActiveStateChangedEventHandler ActiveStateChanged;

    private void Start()
    {
        if (isFirstState)
        {
            ActiveState = this;
        }

        if (SessionSaver.LoadPreviousSession)
        {
            var stateSaver = GetComponentInParent<StateSaver>();
            var savedState = stateSaver.GetState(stateSaver.ActiveStateIndex.Value);
            print("saved state : " + stateSaver.ActiveStateIndex.Value);

            if (ActiveState != savedState)
            { 
                print("ending" + stateSaver.GetStateIndex(ActiveState));
                EndState();
            }
        }
    }

    private void OnValidate()
    {
        desiredNextStateIndex = GetDesiredStateIndex();
    }

    //define which state to chose from the defined possible states
    public virtual int GetDesiredStateIndex()
    {
        return desiredNextStateIndex;
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
        gameObject.SetActive(false);
        StateCompletedAction();
        if (nextState != null)
        {
            nextState.EnterState(this);
        }
    }

    public void EndState()
    {
        EndState(GetDesiredState());
    }

    public void GoToPreviousState()
    {
        gameObject.SetActive(false);
        if (previousState != null)
        {
            previousState.EnterState(previousState.previousState);
            ActiveStateChanged?.Invoke(ActiveState);
        }
    }

    public virtual void StateCompletedAction()
    {
    }

    public void StepEndedByUser()
    {
        EndState();
        ActiveStateChanged?.Invoke(ActiveState);
    }
}
