using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Amsterdam3D.InputHandler
{


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
        void Disable();
        void SubscribePerformed(UnityInputSystemAction.ActionDelegate del, int priority);
        void  SubscribeCancelled(UnityInputSystemAction.ActionDelegate del, int priority);


        bool Used { get; set; }
        bool Performed { get; }
        bool Cancelled { get; }
        string name { get; }
    }

    public class UnityInputSystemAction : IAction
    {
        public bool Used { get; set; }
        public string name { get; private set; }

        public bool Performed { get; private set; }

        public bool Cancelled  { get; private set; }

    public dynamic value;
        public delegate void ActionDelegate(IAction action);
        public List<ActionEventClass> sortedDelegates = new List<ActionEventClass>();



        public T ReadValue<T>() where T: struct
        {
            var returnValue = default(T);
            if (value == null) 
            {
                return returnValue;
            }
            return (T)value;
        }



        public void SetValue(dynamic value)
        {
            this.value = value;
            Used = false;
        }

        public void SetPhase() 
        {

        }


        public void FireEvent()
        {
            //create copy of action so it can't change while handling events
            var action = new UnityInputSystemAction(this.name);
            action.Performed = true;
            action.SetValue(value);

            foreach (var del in sortedDelegates)
            {
                if (del.performed)
                {
                    del.Invoke(action);
                }
            }


            //OnUnityActionEvent?.Invoke(action);
        }

        public void FireCancelEvent()
        {
            //create copy of action so it can't change while handling events
            var action = new UnityInputSystemAction(this.name);
            action.Cancelled = true;
            action.SetValue(value);

            foreach (var del in sortedDelegates)
            {
                if (del.cancelled == true)
                {
                    del.Invoke(action);
                }
            }


            //OnUnityActionEvent?.Invoke(action);
        }


        public void SubscribePerformed(ActionDelegate del, int priority)
        {
            ActionEventClass h = new ActionEventClass(del, priority);
            sortedDelegates.InsertIntoSortedList(h);
        }

        public void Subscribe(ActionDelegate del)
        {
            // add event as lowest priority
            ActionEventClass h = new ActionEventClass(del, sortedDelegates.Count);
            h.performed = true;
            sortedDelegates.Add(h);
        }

        public void Disable()
        {
            
        }

        public void SubscribeCancelled(ActionDelegate del, int priority)
        {
            ActionEventClass h = new ActionEventClass(del, priority);
            h.cancelled = true;
            sortedDelegates.InsertIntoSortedList(h);
        }

        public UnityInputSystemAction(string name)
        {
            this.name = name;
        }


        // nested class otherwise ActionDelegate doesn't work
        // class made to implement IComparable
        public class ActionEventClass : IComparable<ActionEventClass>
        {


            public ActionDelegate del;
            public bool performed;
            public bool cancelled;
            public int priority = 0;




            public ActionEventClass(ActionDelegate del, int priority)
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


    }
}
