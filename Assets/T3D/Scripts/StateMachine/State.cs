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
    public bool IsFirstState => isFirstState;
    [SerializeField, ReadOnly]
    protected int desiredNextStateIndex;

    public static State ActiveState { get; private set; } //allows only 1 state machine throughout the application, maybe change for a dictionary where collections can be defined
    public delegate void ActiveStateChangedEventHandler(State newState);
    public static event ActiveStateChangedEventHandler ActiveStateChangedByUser;

    protected virtual void Awake()
    {
        if (isFirstState)
        {
            ActiveState = this;
        }
    }

    protected virtual void Start()
    {
        if (SessionSaver.LoadPreviousSession)
            LoadSavedState();
    }

    protected virtual void LoadSavedState()
    {
        StateLoadedAction();
        var stateSaver = GetComponentInParent<StateSaver>();
        var savedState = stateSaver.GetState(stateSaver.ActiveStateIndex);
        if (ActiveState != savedState)
        {
            EndState();
        }
    }

    //this breaks the ServiceLocator
    //private void OnValidate()
    //{
    //    desiredNextStateIndex = GetDesiredStateIndex();
    //}

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

    public void EnterStartState()
    {
        if (!IsFirstState)
            Debug.LogError("trying to enter state " + this.ToString() + " but this state is not marked as the first state. Incorrect behaviour can occur");

        EnterState(null);
    }

    // when the state is entered
    protected virtual void EnterState(State previousState)
    {
        ActiveState = this;
        gameObject.SetActive(true);
        this.previousState = previousState;
        StateEnteredAction();
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
        StateCompletedAction();
        if (previousState != null)
        {
            previousState.EnterState(previousState.previousState);
            ActiveStateChangedByUser?.Invoke(ActiveState);
        }
    }

    public virtual void StateLoadedAction()
    {
    }

    public virtual void StateEnteredAction()
    {
    }

    public virtual void StateCompletedAction()
    {
    }

    public void StepEndedByUser()
    {
        EndState();
        ActiveStateChangedByUser?.Invoke(ActiveState);
    }
}
