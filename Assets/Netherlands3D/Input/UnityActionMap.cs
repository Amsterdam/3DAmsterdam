using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
namespace Netherlands3D.InputHandler
{

    public interface IActionMap 
    {
         string name { get; }
        bool enabled { get; }

        List<IAction> boundActions { get; }

        void Enable();
         void Disable();
    }
    
    
    public class UnityActionMap:IActionMap
    {
        public string name { get; private set; }
        public bool enabled { get; private set; }

        public List<IAction> boundActions { get; private set; }
        public UnityEngine.InputSystem.InputActionMap map;
        public void Enable() 
        {
            map.Enable();
            enabled = true;
        }

        public void Disable() 
        {
            map.Disable();
            enabled = false;
        }


        public UnityActionMap(InputActionMap map) 
        {
            this.map = map;
            boundActions = new List<IAction>();
        }


    }
}
