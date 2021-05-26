using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Traffic.VISSIM
{
    public class VissimTypeButton : MonoBehaviour
    {
        public VissimType thisType;
        public VissimTypeUI[] allUITypes = default;
        private Button button = default;
        private Image image = default;

        private void Start()
        {
            button = GetComponent<Button>();
            image = transform.GetChild(0).GetComponent<Image>();
            allUITypes = FindObjectsOfType<VissimTypeUI>();
            button.onClick.AddListener(IsChosen);
            // sets the tool tip name
            this.gameObject.GetComponent<TooltipTrigger>().TooltipText = thisType.name;
            image.sprite = thisType.typeImage;
        }

        /// <summary>
        /// Checks if this vehicle type is chosen and adds the name and types to the object that have been chosen in the interface.
        /// </summary>
        public void IsChosen()
        {
            foreach (VissimTypeUI type in allUITypes)
            {
                // updates the assigned vehicle to the active element
                type.UpdateAssignedVehicles(thisType.vissimTypeAssets, thisType.name);
            }
        }
    }
}