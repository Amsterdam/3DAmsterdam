using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Traffic.VISSIM
{
    public class VissimTypeUI : MonoBehaviour
    {
        public int vissimTypeID;
        public GameObject[] assignedTypes;
        private ConvertFZP fileConverter = default;

        public Toggle toggleObject;
        public Text vissimTypeInfo;

        private void Start()
        {
            fileConverter = FindObjectOfType<ConvertFZP>();

            // creates the buttons, adds them to the list and adds the vissim prefab object.
            toggleObject.group = transform.parent.GetComponent<ToggleGroup>();
            vissimTypeInfo.text = "Voertuigklasse " + vissimTypeID.ToString() + ": <i><color=#004699>Onbekend</color></i>";
        }
        /// <summary>
        /// Updates the assigned VISSIM type and changes the Text in the UI to the new correct type
        /// </summary>
        /// <param name="typeList"></param>
        /// <param name="typeName"></param>
        public void UpdateAssignedVehicles(GameObject[] typeList, string typeName)
        {
            // Checks if the current object has been chosen for editing
            if (toggleObject.isOn)
            {
                // if so then the assigned type is updated
                assignedTypes = typeList;
                // and the text is updated with Richtext enabled
                vissimTypeInfo.text = "Voertuigklasse " + vissimTypeID.ToString() + ": <color=#004699>" + typeName + "</color>";
            }
        }
    }
}