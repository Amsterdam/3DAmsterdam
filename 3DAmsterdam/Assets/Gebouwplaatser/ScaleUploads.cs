using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleUploads : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> gameObjects;

    public GameObject scaleMenu;
    public Slider height, width, length;

    private GameObject selectedObject;
    private bool turnOffMenu;

    public void Start()
    {
        height.value = 0.5f;
        width.value = 0.5f;
        length.value = 0.5f;
    }

    private void Update()
    {
        turnOffMenu = true;

        // alle objecten die verschaalt kunnen worden worden in deze lijst toegevoegd
        for (int i = 0; i < gameObjects.Count; i++)
        {
            // als de pijlenprefab geselecteerd is wordt het schaalmenu geopend
            if (gameObjects[i].gameObject.GetComponent<PijlenPrefab>().scaling)
            {
                scaleMenu.SetActive(true);
                selectedObject = gameObjects[i].gameObject;

                if (selectedObject.GetComponent<PijlenPrefab>().setScaleValues)
                {
                    InitializeScaleValues();
                }

                turnOffMenu = false;
            }    
        }

        // het menu wordt uigezet zodra de pijlenprefab gedeselecteerd is
        if (turnOffMenu)
        {
            scaleMenu.SetActive(false);
        }

        Scaling();
    }

    // het verschalen van het object
    private void Scaling()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale = new Vector3(width.value * 50f, height.value * 50f, length.value * 50f);
        }
    }

    // de huidige waardes van het object worden overgenomen in het schaalmenu
    private void InitializeScaleValues()
    {
        height.value = selectedObject.transform.localScale.y / 50f;
        width.value = selectedObject.transform.localScale.x / 50f;
        length.value = selectedObject.transform.localScale.z / 50f;
    }
}
