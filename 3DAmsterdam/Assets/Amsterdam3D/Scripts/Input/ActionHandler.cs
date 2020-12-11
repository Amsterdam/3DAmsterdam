using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Amsterdam3D.InputHandler
{
    /// <summary>
    /// Class that handles input events from Unity's input system. Subscribing to input events should be done in this class or in UnityAction, rather than in 
    /// InputAction directly.
    /// </summary>
    /// 
    //TODO: When old input system is needed, replace parts in this script that require new input system
    public class ActionHandler:MonoBehaviour
    {

        public static _3DAmsterdam actions;
        InputActionAsset actionMaps;


        public static ActionHandler instance;


        public Dictionary<InputAction, UnityInputSystemAction> ActionDictionary = new Dictionary<InputAction, UnityInputSystemAction>();
        public List<UnityActionMap> unityActionMaps = new List<UnityActionMap>();


        private bool inputEnabled = true;


        private void Awake()
        {

            instance = this;
            actions = new _3DAmsterdam();
            actionMaps = actions.asset; 
           
            foreach (UnityEngine.InputSystem.InputActionMap map in actionMaps.actionMaps) 
            {
                UnityActionMap unityMap = new UnityActionMap(map);
                foreach (var inputAction in map) 
                {
                    UnityInputSystemAction tmp = new UnityInputSystemAction(inputAction.name);

                    ActionDictionary.Add(inputAction, tmp);
                    inputAction.performed += Inputaction_performed;
                    inputAction.canceled += InputAction_canceled;
                    unityMap.boundActions.Add(ActionDictionary[inputAction]);
                    ActionHandler.actions.StreetView.Disable();
                }
                unityActionMaps.Add(unityMap);

                
               
            } 

        }

        private void InputAction_canceled(InputAction.CallbackContext obj)
        {
            // could be faster if it didn't have to search a dictionary?
            UnityInputSystemAction action = ActionDictionary[obj.action];
            action.SetValue(obj.action.ReadValueAsObject());
        }

        private void Inputaction_performed(InputAction.CallbackContext obj)
        {
            // could be faster if it didn't have to search a dictionary?
            UnityInputSystemAction action = ActionDictionary[obj.action];
            action.SetValue(obj.action.ReadValueAsObject());

            // Fire Event on UnityAction
            action.FireEvent();
        }


        public bool subscribeToAction(string actionName, UnityInputSystemAction.ActionDelegate func) 
        {
            foreach(var action in ActionDictionary.Keys) 
            {
                if (action.name == actionName) 
                {
                    ActionDictionary[action].Subscribe(func);
                    return true;
                }
            }

            //not in dictionary, return false
            return false;
        }



        public bool SubscribeToAction(InputAction action, UnityInputSystemAction.ActionDelegate func, bool AddWhenNotExisting = false) 
        {
            if (!ActionDictionary.ContainsKey(action)) 
            {
                if (AddWhenNotExisting)
                {
                    UnityInputSystemAction newAction = new UnityInputSystemAction(action.name);
                    ActionDictionary.Add(action, newAction);
                    newAction.Subscribe(func);
                }
                else 
                {
                    return false;
                }
            }

            ActionDictionary[action].Subscribe(func);
            return true;
        }


        public IAction GetAction(InputAction action) 
        {
            return ActionDictionary[action];
        }

        public IAction GetAction(string actionName) 
        {
            foreach (UnityInputSystemAction action in ActionDictionary.Values) 
            {
                if (action.name == actionName) 
                {
                    return action;
                }
            }
            return null;
        }


        //still a bit slow 
        public IActionMap GetActionMap(InputActionMap map) 
        {
            foreach (var actionMap in unityActionMaps) 
            {
                if (actionMap.map == map) 
                {
                    return actionMap;
                }
            }
            return null;
        }


        public void EnableInputSystem(bool enabled) 
        {
            this.inputEnabled = enabled;
        }


    }
}
