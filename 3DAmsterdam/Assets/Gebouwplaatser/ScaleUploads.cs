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

    private void Update()
    {
        turnOffMenu = true;

        for (int i = 0; i < gameObjects.Count; i++)
        {
            if (gameObjects[i].gameObject.GetComponent<PijlenPrefab>().scaling)
            {
                scaleMenu.SetActive(true);
                selectedObject = gameObjects[i].gameObject;

                height.value = selectedObject.transform.localScale.y / 50f;
                width.value = selectedObject.transform.localScale.x / 50f;
                length.value = selectedObject.transform.localScale.z / 50f;

                turnOffMenu = false;
            }
        }

        if (turnOffMenu)
        {
            scaleMenu.SetActive(false);
        }

        Scaling();
    }

    private void Scaling()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.localScale = new Vector3(width.value * 50f, height.value * 50f, length.value * 50f);
        }
    }
}
