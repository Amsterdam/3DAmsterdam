using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleUploads : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> gameObjects;

    public GameObject subMenu;

    private MenuFunctions menu;
    
    private void Start()
    {
        menu = GameObject.Find("Menus").GetComponent<MenuFunctions>();
    }

    private void Update()
    {
        for (int i=0; i<gameObjects.Count; i++)
        {
            if (gameObjects[i].gameObject.GetComponent<PijlenPrefab>().scaling)
            {
                menu.currentMenu = 2;
                subMenu.SetActive(true);
            }
        }
    }
}
