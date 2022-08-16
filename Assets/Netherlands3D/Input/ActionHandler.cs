using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Netherlands3D.InputHandler
{
    /// <summary>
    /// Class that handles input events from Unity's input system. Subscribing to input events should be done in this class or in UnityAction, rather than in 
    /// InputAction directly.
    /// </summary>
    /// 
    //TODO: When old input system is needed, replace parts in this script that require new input system
    [DefaultExecutionOrder(-1000)]
    public class ActionHandler : MonoBehaviour, IUniqueService
    {

        public static Netherlands3DInputActions actions;
        InputActionAsset actionMaps;

        public Dictionary<InputAction, UnityInputSystemAction> ActionDictionary = new Dictionary<InputAction, UnityInputSystemAction>();
        public List<UnityActionMap> unityActionMaps = new List<UnityActionMap>();


        private bool inputEnabled = true;


        private void Awake()
        {
            actions = new Netherlands3DInputActions();
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
                    inputAction.started += Inputaction_started;
                    unityMap.boundActions.Add(ActionDictionary[inputAction]);
                    ActionHandler.actions.StreetView.Disable();
                }
                unityActionMaps.Add(unityMap);



            }

        }

        private void OnDestroy()
        {
            foreach (UnityEngine.InputSystem.InputActionMap map in actionMaps.actionMaps)
            {
                UnityActionMap unityMap = new UnityActionMap(map);
                foreach (var inputAction in map)
                {
                    UnityInputSystemAction tmp = new UnityInputSystemAction(inputAction.name);

                    inputAction.performed -= Inputaction_performed;
                    inputAction.canceled -= InputAction_canceled;
                    inputAction.started -= Inputaction_started;
                }
                unityMap.boundActions.Clear();
            }
            unityActionMaps.Clear();
            ActionDictionary.Clear();
        }

        private void InputAction_canceled(InputAction.CallbackContext obj)
        {
            // could be faster if it didn't have to search a dictionary?
            UnityInputSystemAction action = ActionDictionary[obj.action];
            action.SetValue(obj.action.ReadValueAsObject());

            action.FireCancelEvent();
        }

        private void Inputaction_performed(InputAction.CallbackContext obj)
        {
            // could be faster if it didn't have to search a dictionary?
            UnityInputSystemAction action = ActionDictionary[obj.action];
            action.SetValue(obj.action.ReadValueAsObject());

            // Fire Event on UnityAction
            action.FirePerformedEvent();
        }

        private void Inputaction_started(InputAction.CallbackContext obj)
        {
            // could be faster if it didn't have to search a dictionary?
            UnityInputSystemAction action = ActionDictionary[obj.action];
            action.SetValue(obj.action.ReadValueAsObject());

            // Fire Event on UnityAction
            action.FireStartedEvent();
        }



        /// <summary>
        /// Subscribe to IAction Performed without returning IAction.
        /// Returns true if succesful and false if Action doesn't exist.
        /// </summary>
        public bool SubscribeToActionPerformed(string actionName, UnityInputSystemAction.ActionDelegate func)
        {
            foreach (var action in ActionDictionary.Keys)
            {
                if (action.name == actionName)
                {
                    ActionDictionary[action].SubscribePerformed(func);
                    return true;
                }
            }

            //not in dictionary, return false
            return false;
        }




        /// <summary>
        /// Subscribe to IAction performed without returning IAction.
        /// Returns true if succesful and false if Action doesn't exist.
        /// </summary>
        public bool SubscribeToActionPerformed(InputAction action, UnityInputSystemAction.ActionDelegate func, bool AddWhenNotExisting = false)
        {
            if (!ActionDictionary.ContainsKey(action))
            {
                if (AddWhenNotExisting)
                {
                    UnityInputSystemAction newAction = new UnityInputSystemAction(action.name);
                    ActionDictionary.Add(action, newAction);
                    newAction.SubscribePerformed(func);
                }
                else
                {
                    return false;
                }
            }

            ActionDictionary[action].SubscribePerformed(func);
            return true;
        }




        /// <summary>
        /// Gets the corresponding Action class by either name or by Unity input system Action.
        /// returns null if Action doesn't exist.
        /// </summary>
        public IAction GetAction(InputAction action)
        {
            return ActionDictionary[action];
        }

        /// <summary>
        /// Gets the corresponding Action class by either name or by Unity input system Action.
        /// returns null if Action doesn't exist.
        /// </summary>
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


        /// <summary>
        /// Gets the corresponding Action map class, to enable or disable, or get actions from
        /// </summary>
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


        /// <summary>
        /// Enables or disables all input. 
        /// </summary>
        public void EnableInputSystem(bool enabled)
        {
            this.inputEnabled = enabled;
        }


    }
}
