﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class VissimConfiguration : MonoBehaviour
    {
        private ConvertFZP fileConverter = default;
        private List<VissimTypeUI> vehicleTypeUI = new List<VissimTypeUI>();

        [SerializeField] private GameObject vissimTypesPrefab = default;
        [SerializeField] private GameObject vissimTypeInstance = default;

        [SerializeField] private GameObject vissimChooseTypesButton = default;
        [SerializeField] private GameObject vissimStartVissim = default;
        private void Start()
        {
            fileConverter = GetComponent<ConvertFZP>();
        }
        /// <summary>
        /// Opens missing Vissim files interface
        /// </summary>
        /// <param name="missingVehiclesTypes"></param>
        public void OpenInterface(List<int> missingVehiclesTypes)
        {
            // Opens the side panel
            ObjectProperties.Instance.OpenPanel("Vissim instellingen");
            ObjectProperties.Instance.AddTitle("Onbekende voertuigklasse koppelen");
            ObjectProperties.Instance.AddURLText("Meer info", "https://3d.amsterdam.nl/web/wat%20is%20nieuw/documentatie/VISSIM_3DAmsterdam.pdf");
            ObjectProperties.Instance.AddDataField("Aantal voertuigen", fileConverter.vehicleTypes.Count.ToString());
            ObjectProperties.Instance.AddDataField("Aantal onbekend", fileConverter.missingVissimTypes.Count.ToString());
            ObjectProperties.Instance.AddCustomField(vissimTypesPrefab);
            ObjectProperties.Instance.AddCustomField(vissimChooseTypesButton);
            ObjectProperties.Instance.AddCustomField(vissimStartVissim);
            
            Transform tempInterface = FindObjectOfType<ToggleGroup>().transform;
            
            foreach (int type in missingVehiclesTypes)
            {
                // spawn ui element as child
                GameObject tempObject = Instantiate(vissimTypeInstance, tempInterface.position, tempInterface.rotation);
                tempObject.transform.SetParent(tempInterface.transform);
                // gets the UI element and adds the correct id type
                VissimTypeUI tempTypeUI = tempObject.GetComponent<VissimTypeUI>();
                tempTypeUI.vissimTypeID = type;
                vehicleTypeUI.Add(tempTypeUI);
                
            }
            
        }

        public void SetVehicles()
        {
            // foreach vehicleTypeUI from  lijst
            foreach (VissimTypeUI vehicleTypeItem in vehicleTypeUI)
            {
                if (!fileConverter.vehicleTypes.ContainsKey(vehicleTypeItem.vissimTypeID))
                {
                    fileConverter.vehicleTypes.Add(vehicleTypeItem.vissimTypeID, vehicleTypeItem.assignedTypes);
                }
                else
                {
                    fileConverter.vehicleTypes[vehicleTypeItem.vissimTypeID] = vehicleTypeItem.assignedTypes;
                }
            }
            fileConverter.missingVissimTypes.Clear();
            // close panel
            ObjectProperties.Instance.ClearGeneratedFields();
            ObjectProperties.Instance.ClosePanel();
            fileConverter.StartVissim();
        }
    }
}