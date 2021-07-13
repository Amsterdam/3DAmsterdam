using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.JavascriptConnection {
    public class ChangePointerStyleHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum Style{
            AUTO,    
            POINTER,
            TEXT,
            GRAB,
            GRABBING,
            ERESIZE,
            PROGRESS
        }

        [SerializeField]
        private Style styleOnHover = Style.POINTER;
        public static Style cursorType = Style.AUTO;

		public Style StyleOnHover { get => styleOnHover; set => styleOnHover = value; }

		//We can use a static method here, since we change the style as a whole for the 
		//entire WebGL canvas. Check out w3schools for more cursor styles: https://www.w3schools.com/cssref/pr_class_cursor.asp
		public static void ChangeCursor(Style type)
        {
            cursorType = type;

            var cursorString = "";
            switch (cursorType)
            {
                case Style.AUTO:
                    cursorString = "auto";
                    break;
                case Style.POINTER:
                    cursorString = "pointer";
                    break;
                case Style.TEXT:
                    cursorString = "text";
                    break;
                case Style.GRAB:
                    cursorString = "grab";
                    break;
                case Style.GRABBING:
                    cursorString = "grabbing";
                    break;
                case Style.ERESIZE:
                    cursorString = "e-resize";
                    break;
                case Style.PROGRESS:
                    cursorString = "progress";
                    break;
            }
            JavascriptMethodCaller.ChangeCursor(cursorString);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ChangeCursor(StyleOnHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Always change back cursor to CSS default 'auto'
            ChangeCursor(Style.AUTO);
        }

		public void OnDisable()
		{
            ChangeCursor(Style.AUTO);
        }
	}
}
