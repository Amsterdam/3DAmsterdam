using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Amsterdam3D.InputSystem
{

    public interface IAction 
    {
         T ReadValue<T>();


        void SetValue(dynamic value);


         bool Used { get; set; }
        string name { get; }
    }

    public class UnityAction:IAction
    {
        public bool Used { get; set; }
        public string name { get; private set; }
        public dynamic value;


        public delegate void ActionDelegate(IAction action);
        public event ActionDelegate OnUnityActionEvent;

        public List<UnityAction.ActionDelegate> sortedDelegates = new List<UnityAction.ActionDelegate>();



        public T ReadValue<T>()
        {  
            return (T)value;
        }



        public void SetValue(dynamic value)
        {
            this.value = value;
            Used = false;
        }


        public void FireEvent() 
        {
            //create copy of action so it can't change while handling events
            var action = new UnityAction(this.name);
            action.SetValue(value);

            foreach (var del in sortedDelegates) 
            {
                del.Invoke(action);
            }


            OnUnityActionEvent?.Invoke(action);
        }

        public void Subscribe(ActionDelegate del, int priority) 
        {
            sortedDelegates.InsertIntoSortedList(del, priority);
        }

        public void Subscribe(ActionDelegate del) 
        {
            sortedDelegates.Add(del);
        }


        public UnityAction(string name) 
        {
            this.name = name;
        }

    }


}
