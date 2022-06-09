using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D
{
    public class OverallProgressIndicator : MonoBehaviour
    {
        private Image fillImage;
 
        [Header("Listen to")]
        [SerializeField]
        private GameObjectEvent working;
        [SerializeField]
        private GameObjectEvent doneWorking;

        [SerializeField]
        private List<object> workingObjects = new List<object>();

        void Awake()
        {
            fillImage = GetComponent<Image>();

            working.started.AddListener(Working);
            doneWorking.started.AddListener(Done);
            Show(false);
        }

        private void Working(GameObject workingObject)
        {
            if (!workingObjects.Contains(workingObject))
                workingObjects.Add(workingObject);

            Show(true);

#if UNITY_EDITOR
            Debug.Log($"{workingObject.name} is working...", workingObject);
#endif
        }

        private void Done(GameObject doneWorkingObject)
        {
            if(workingObjects.Contains(doneWorkingObject))
                workingObjects.Remove(doneWorkingObject);

            if (workingObjects.Count == 0)
                Show(false);

#if UNITY_EDITOR
            Debug.Log($"{doneWorkingObject.name} is done working.", doneWorkingObject);
#endif
        }


        /// <summary>
        /// Shows a general progress spinner on screen
        /// </summary>
        /// <param name="showIndicator">Show indicator on screen</param>
        public void Show(bool showIndicator)
        {
            fillImage.gameObject.SetActive(showIndicator);
        }
    }
}