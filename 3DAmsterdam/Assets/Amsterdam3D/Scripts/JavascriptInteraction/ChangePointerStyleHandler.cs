using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Amsterdam3D.JavascriptConnection {
    public class ChangePointerStyleHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private string cursorType = "pointer";
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            JavascriptMethodCaller.ChangeCursor(cursorType);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Always change back cursor to CSS default 'auto'
            JavascriptMethodCaller.ChangeCursor("auto");
        }
    }
}
