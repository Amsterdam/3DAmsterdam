using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Netherlands3D.InputHandler
{

    // class made to implement IComparable
    public class ActionEventClass : IComparable<ActionEventClass>
    {
        public UnityInputSystemAction.ActionDelegate del;
        public bool Performed;
        public bool Cancelled;
        public bool Started;
        public int priority = 0;

        public ActionEventClass(UnityInputSystemAction.ActionDelegate del, int priority)
        {
            this.del = del;
            this.priority = priority;
        }

        public void Invoke(IAction action)
        {
            del?.Invoke(action);
        }

        public int CompareTo(ActionEventClass other)
        {
            return this.priority - other.priority;
        }
    }

    public enum ActionPhase 
    {
        Idle,
        Performed,
        Cancelled
    }
    
    public interface IAction
    {
        T ReadValue<T>() where T : struct;

        void SetValue(dynamic value);

        /// <summary>
        /// Subscribe to the performed event. Fires when Unity's input system's interaction is triggered.
        /// Make sure to configure Unity's Input action's Interaction properly so performed gets called when intended. 
        ///  For more details, check Unity's Input system docs. 
        /// </summary>
        ActionEventClass SubscribePerformed(UnityInputSystemAction.ActionDelegate del, int priority = 0);
        /// <summary>
        /// Subscribe to the cancelled event. Triggers when the action is stopped.
        ///  For more details, check Unity's Input system docs. 
        /// </summary>
        ActionEventClass SubscribeCancelled(UnityInputSystemAction.ActionDelegate del, int priority = 0);
        /// <summary>
        /// Subscribe to the started event. Triggers when the action is started. Triggers before performed. 
        ///  For more details, check Unity's Input system docs. 
        /// </summary>
        ActionEventClass SubscribeStarted(UnityInputSystemAction.ActionDelegate del, int priority = 0);

        void UnSubscribe(ActionEventClass ev);

        bool Used { get; set; }
        bool Performed { get; }
        bool Cancelled { get; }

        bool Started { get; }
        string name { get; }
    }

    public class UnityInputSystemAction : IAction
    {
        public bool Used { get; set; }
        public string name { get; private set; }

        public bool Performed { get; private set; }

        public bool Cancelled  { get; private set; }

        public bool Started { get; private set; }

        public object value;
        public delegate void ActionDelegate(IAction action);
        public List<ActionEventClass> sortedDelegates = new List<ActionEventClass>();
        private bool enabled;
        public T ReadValue<T>() where T: struct
        {
            var returnValue = default(T);
            if (value == null) 
            {
                return returnValue;
            }
            return (T)value;
        }

        public void SetValue(object value)
        {
            this.value = value;
            Used = false;
        }
        public void FirePerformedEvent()
        {
            //create copy of action so it can't change while handling events
            var action = new UnityInputSystemAction(this.name);
            action.Performed = true;
            action.SetValue(value);

            foreach (var del in sortedDelegates)
            {
                if (del.Performed)
                {
                    del.Invoke(action);
                }
            }
        }

        public void FireCancelEvent()
        {
            //create copy of action so it can't change while handling events
            var action = new UnityInputSystemAction(this.name);
            action.Cancelled = true;
            action.SetValue(value);

            foreach (var del in sortedDelegates)
            {
                if (del.Cancelled == true)
                {
                    del.Invoke(action);
                }
            }
        }

        public void FireStartedEvent()
        {
            //create copy of action so it can't change while handling event
            var action = new UnityInputSystemAction(this.name);
            action.Started = true;
            action.SetValue(value);

            foreach (var del in sortedDelegates)
            {
                if (del.Started == true)
                {
                    del.Invoke(action);
                }
            }
        }

        public ActionEventClass SubscribePerformed(ActionDelegate del, int priority = 0)
        {
            ActionEventClass eventClass = new ActionEventClass(del, sortedDelegates.Count);
            eventClass.Performed = true;
            sortedDelegates.Add(eventClass);

            return eventClass;
        }

        public ActionEventClass SubscribeCancelled(ActionDelegate del, int priority = 0)
        {
            ActionEventClass eventClass = new ActionEventClass(del, priority);
            eventClass.Cancelled = true;
            sortedDelegates.InsertIntoSortedList(eventClass);
            return eventClass;
        }

        public ActionEventClass SubscribeStarted(ActionDelegate del, int priority = 0)
        {
            ActionEventClass eventClass = new ActionEventClass(del, priority);
            eventClass.Started = true;
            sortedDelegates.InsertIntoSortedList(eventClass);

            return eventClass;
        }

        public void UnSubscribe(ActionEventClass ev)
        {
            sortedDelegates.Remove(ev);
        }

        public UnityInputSystemAction(string name)
        {
            this.name = name;
        }
    }
}
