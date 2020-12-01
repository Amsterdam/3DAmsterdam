using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
namespace Amsterdam3D.InputSystem
{

    public interface IActionMap 
    {
         string name { get; }
        bool enabled { get; }

        void Enable();
         void Disable();
    }
    
    
    public class UnityActionMap:IActionMap
    {
        public string name { get; private set; }
        public bool enabled { get; private set; }

        public List<UnityAction> boundActions = new List<UnityAction>();
        //decouple this part of the code
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
        }


    }
}
