using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CreateDataAccordion : MonoBehaviour
{
    [SerializeField] private GameObject dataAccordion;
    [SerializeField] private GameObject dataChildAccordion;
    [SerializeField] private RootAccordionController rootAccordion;

    [SerializeField] private Transform container;

    public void CreateObject(GameObject linkedObject)
    {
        Debug.Log($"WAS HERE 2 --> {linkedObject}");

        var accordion = Instantiate(dataAccordion, container);
        accordion.GetComponent<DefaultAccordionController2>().ToggleEnabled = true;
        var dataController = accordion.GetComponent<DataAccordionController>();
        dataController.SetFields(true, dataChildAccordion, linkedObject, true);

        rootAccordion.SetupAccordion();

    }

}
