using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class InterfaceLayers : MonoBehaviour
    {
        [SerializeField]
        private CustomLayer customObjectLayerPrefab;

        [SerializeField]
        private RectTransform layersContainer;

        private Animator animator;
        private bool toggledVisible = false;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }
        
        public void ToggleVisibility(bool visible)
        {
            toggledVisible = visible;
            animator.SetBool("AnimateIn", toggledVisible);
        }

        public void AddNewCustomObjectLayer(GameObject linkedWorldObject, CustomLayerType type){
            CustomLayer newCustomlayer = Instantiate<CustomLayer>(customObjectLayerPrefab, layersContainer);
            newCustomlayer.Create(linkedWorldObject.name, linkedWorldObject, type);
            newCustomlayer.transform.SetSiblingIndex(0);
        }
    }
}
