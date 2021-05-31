using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class SelectorInterfaceInteractable : Interactable
    {
        [SerializeField]
        private Button closeWindowButton;

        void OnEnable()
        {
            TakeInteractionPriority();
        }

		public override void Escape()
        {
            //Simply force the close click of the window panel, to share logic.
            closeWindowButton.onClick.Invoke();
        }
    }
}