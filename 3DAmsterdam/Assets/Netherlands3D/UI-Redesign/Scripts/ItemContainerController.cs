using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Netherlands3D.Interface.SidePanel;
public class ItemContainerController : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;

    public void SetLinkedObject(GameObject linkedObject)
    {
        Instantiate(buttonPrefab, transform).GetComponent<ActionButton>().LinkedObject = linkedObject;
    }
}
