using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Amsterdam3D.InputSystem
{
  public  class ActionHandlerTest:MonoBehaviour
    {

        IAction polledAction;
        bool gotAction;
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) 
            {
               Debug.Log( ActionHandler.instance.SubscribeToAction(ActionHandler.actions.StreetView.Move, Test));
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter)) 
            {
                if (ActionHandler.instance.GetActionMap(ActionHandler.actions.StreetView).enabled)
                {
                    ActionHandler.instance.GetActionMap(ActionHandler.actions.StreetView).Disable();
                    Debug.Log("Streetview Action map disabled");
                }
                else 
                {
                    ActionHandler.instance.GetActionMap(ActionHandler.actions.StreetView).Enable();
                    Debug.Log("Streetview Action map enabled");
                }
            }


            if (Input.GetKeyDown(KeyCode.Return)) 
            {
                if (gotAction) 
                {
                    gotAction = false;
                }
                polledAction = ActionHandler.instance.GetAction(ActionHandler.actions.StreetView.Move);
                gotAction = true;
            }

            if (gotAction) 
            {
                Debug.Log("Polled action value is: " + polledAction.ReadValue<Vector2>());
            }


        }

        private void Test(IAction action)
        {
            Debug.Log("Test event succeeded with value " + action.ReadValue<Vector2>() + "Is Used: " + action.Used);
            action.Used = true;
          
        }

        private void Test(UnityAction action) 
        {
            
        }
    }
}
