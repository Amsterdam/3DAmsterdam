using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void UpdateAssignedVehicles(GameObject[] typeList, string typeName)
    {
        if (toggleObject.isOn)
        {
            assignedTypes = typeList;

            vissimTypeInfo.text = "Voertuigklasse " + vissimTypeID.ToString() + ": <color=#004699>" + typeName + "</color>";
        }
    }
}
