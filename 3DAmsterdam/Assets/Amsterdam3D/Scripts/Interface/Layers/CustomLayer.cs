using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public enum CustomLayerType
    {
        BASICSHAPE,
        OBJMODEL
    }
    public class CustomLayer : MonoBehaviour
    {       
        private CustomLayerType layerType = CustomLayerType.BASICSHAPE;
        private GameObject linkedObject;

        [SerializeField]
        private Text layerNameText;

        public void Remove()
        {
            //TODO: A confirmation before removing might be required. Verify.
            Destroy(gameObject);
        }

        public void Create(string name, GameObject link, CustomLayerType type)
        {
            layerType = type;
            layerNameText.text = name;
            linkedObject = link;
        }

        public void ToggleLinkedObject(bool isOn){
            linkedObject.SetActive(isOn);
        }

        private void OnDestroy()
        {
            GameObject.Destroy(linkedObject);
        }
    }
}