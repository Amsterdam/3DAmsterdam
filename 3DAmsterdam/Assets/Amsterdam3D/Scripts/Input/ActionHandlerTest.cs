using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Amsterdam3D.InputHandler;
namespace Amsterdam3D.InputHandler
{
  public  class ActionHandlerTest:MonoBehaviour
    {

        InputAction polledAction;
        bool gotAction;
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) 
            {
               // high priority action
                 ActionHandler.instance.GetAction(ActionHandler.actions.StreetView.Move).SubscribePerformed(Test, 0);
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
                ActionHandler.instance.GetAction(ActionHandler.actions.StreetView.Move).SubscribePerformed(LowPriorityTest, 1);
            }



        }

        private void Test(IAction action)
        {
            Debug.Log("Test event succeeded with value " + action.ReadValue<Vector2>() + "Is Used: " + action.Used);
            action.Used = true;
          
        }

        private void LowPriorityTest(IAction action) 
        {
            Debug.Log("Low priority Test event succeeded with value " + action.ReadValue<Vector2>() + "Is Used: " + action.Used);
        }

    }
}
