using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Amsterdam3D.InputSystem
{
   public class ActionHandler:MonoBehaviour
    {

        public static _3DAmsterdam actions;
        [SerializeField]
        InputActionAsset actionMaps;


        public static ActionHandler instance;


        public Dictionary<InputAction, UnityAction> ActionDictionary = new Dictionary<InputAction, UnityAction>();
        public List<UnityActionMap> unityActionMaps = new List<UnityActionMap>();

        private void Start()
        {

            instance = this;
            actions = new _3DAmsterdam();
            actionMaps = actions.asset; 
            actionMaps.Enable();
            foreach (UnityEngine.InputSystem.InputActionMap map in actionMaps.actionMaps) 
            {
                UnityActionMap unityMap = new UnityActionMap(map);
                foreach (var inputAction in map) 
                {
                    ActionDictionary.Add(inputAction, new UnityAction(inputAction.name));
                    inputAction.performed += Inputaction_performed;
                    unityMap.boundActions.Add(ActionDictionary[inputAction]);
                }
                unityActionMaps.Add(unityMap);

                
               
            }

        }

        private void Inputaction_performed(InputAction.CallbackContext obj)
        {
            UnityAction action = ActionDictionary[obj.action];
            action.SetValue(obj.action.ReadValueAsObject());

            // Fire Event on UnityAction
            action.FireEvent();
        }


        public bool subscribeToAction(string actionName, UnityAction.ActionDelegate func) 
        {
            foreach(var action in ActionDictionary.Keys) 
            {
                if (action.name == actionName) 
                {
                    ActionDictionary[action].Subscribe(func, 0);
                    return true;
                }
            }

            //not in dictionary, return false
            return false;
        }



        public bool SubscribeToAction(InputAction action, UnityAction.ActionDelegate func, bool AddWhenNotExisting = false) 
        {
            if (!ActionDictionary.ContainsKey(action)) 
            {
                if (AddWhenNotExisting)
                {
                    ActionDictionary.Add(action, new UnityAction(action.name));
                }
                else 
                {
                    return false;
                }
            }

            ActionDictionary[action].OnUnityActionEvent += func;
            return true;
        }


        public IAction GetAction(InputAction action) 
        {
            return ActionDictionary[action];
        }

        public IAction GetAction(string actionName) 
        {
            foreach (UnityAction action in ActionDictionary.Values) 
            {
                if (action.name == actionName) 
                {
                    return action;
                }
            }
            return null;
        }


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


        public void DisableInputSystem() 
        {

        }
        public void Update()
        {
            //update input system manually, can be disabled if needed
            UnityEngine.InputSystem.InputSystem.Update();

        }

    }
}
