using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuFunctions : MonoBehaviour
{
    public GameObject[] allMenus;
    public Button[] buttons;
    private int currentMenu;
    private int noMenu = 10;



    public GameObject menus;

    MenuExtender extensionScript;

    public Toggle OptionsToggleOne;
    public Toggle OptionsToggleTwo;
    public GameObject coordinatesPanel;
    public GameObject miniMap;

    private void Start()
    {
        currentMenu = noMenu;
    }

    private void Update()
    {
        // het huidige menu wordt zichtbaar gemaakt
        if (currentMenu != noMenu) allMenus[currentMenu].SetActive(true);
        
        // alle andere menus worden ontzichtbaar gemaakt.
        for (int i = 0; i < allMenus.Length; i++)
        {
            if (i != currentMenu) allMenus[i].SetActive(false);
        }
    }

    public void One()
    {
        DecideMenu(0);
        Back(allMenus[0], buttons[0]);
    }

    public void Two()
    {
        DecideMenu(1);
        Back(allMenus[1], buttons[1]);
    }

    public void Three()
    {
        DecideMenu(2);
        Back(allMenus[2], buttons[2]);
    }

    public void Four()
    {
        DecideMenu(3);
        Back(allMenus[3], buttons[3]);
    }


    // kiest het huidige menu
    private void DecideMenu(int _currentMenu)
    {
        currentMenu = _currentMenu;
    }

    private void Back(GameObject menu, Button button)
    {
        ColorBlock colors = button.colors;

        if (menu.activeSelf)
        {
            currentMenu = noMenu;

            colors.highlightedColor = new Color(0.094f, 0.094f, 0.094f);
            button.colors = colors;
        }
        else
        {
            colors.highlightedColor = Color.red;
            button.colors = colors;
        }
    }

    // zet minimap en coordinates panelen uit/aan onder opties menu
    public void ToggleCoordinates()
    {
        if (OptionsToggleOne.GetComponent<Toggle>().isOn)
        {
            coordinatesPanel.SetActive(true);
        } else
        {
            coordinatesPanel.SetActive(false);
        }

        if (OptionsToggleTwo.GetComponent<Toggle>().isOn)
        {
            miniMap.SetActive(true);
        } else
        {
            miniMap.SetActive(false);
        }
    }

}
