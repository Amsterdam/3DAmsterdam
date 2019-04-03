using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scaling : MonoBehaviour
{
    private float growthFactor = 1.01f;
    private float shrinkFactor = 0.99f;

    Vector3 originalSize;

    public GameObject button;
    private Image buttonImage;

    void Start()
    {
        originalSize = transform.parent.parent.parent.localScale; // de startgrootte van het object
        buttonImage = button.GetComponent<Image>();
    }

    public void OriginalSize()
    {
        transform.parent.parent.parent.localScale = originalSize;

        EventSystem.current.SetSelectedGameObject(null); // zorgt dat de knop niet 'highlighted' blijft na klikken
    }

    public void ScalingObject()
    {
        ManageStates.Instance.selectionState = "Scaling";
    }

    void Update()
    {
        if (ManageStates.Instance.selectionState == "Scaling")
        {
            if (Input.GetKey(KeyCode.UpArrow)) transform.parent.parent.parent.localScale *= growthFactor;
            if (Input.GetKey(KeyCode.DownArrow)) transform.parent.parent.parent.localScale *= shrinkFactor;

            buttonImage.color = Color.yellow;
        }
        else
        {
            buttonImage.color = Color.white;
        }
    }
}
