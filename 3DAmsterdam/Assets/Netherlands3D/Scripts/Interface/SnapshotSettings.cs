using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
{
    public class SnapshotSettings : Interactable
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            TakeInteractionPriority();
        }
    }
}