using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface{ 
public class InterfaceLayerCreator : MonoBehaviour
{
        [SerializeField]
        private GameObjectEvent onCreateLayerRequest;

        void Start()
        {
            onCreateLayerRequest.AddListenerStarted(CreateLayerForGameObject);
        }

		private void CreateLayerForGameObject(GameObject linkedGameObject)
		{
			
		}

		void Update()
        {
        
        }
    }
}