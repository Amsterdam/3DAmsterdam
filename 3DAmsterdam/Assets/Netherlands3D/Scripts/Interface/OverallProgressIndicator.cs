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
        [SerializeField]
        private Image fillImage;
 
        [Header("Listen to")]
        [SerializeField]
        private GameObjectEvent working;
        [SerializeField]
        private GameObjectEvent doneWorking;

        [SerializeField]
        private List<GameObject> workingObjects = new List<GameObject>();

        void Awake()
        {
            working.started.AddListener(Working);
            doneWorking.started.AddListener(Done);
            Show(false);
        }

        private void Working(GameObject workingObject)
        {
            if (!workingObjects.Contains(workingObject))
                workingObjects.Add(workingObject);

            Show(true);
        }

        private void Done(GameObject doneWorkingObject)
        {
            if(workingObjects.Contains(doneWorkingObject))
                workingObjects.Remove(doneWorkingObject);

            if (workingObjects.Count == 0)
                Show(false);
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