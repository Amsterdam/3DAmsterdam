using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuFunctions : MonoBehaviour
{
    public GameObject[] allMenus;

    public GameObject menus;
    private int currentMenu;

    MenuExtender extensionScript;

    public Toggle OptionsToggleOne;
    public Toggle OptionsToggleTwo;
    public GameObject coordinatesPanel;
    public GameObject miniMap;

    private void Start()
    {
        extensionScript = menus.GetComponent<MenuExtender>();
    }

    private void Update()
    {
        // het huidige menu wordt zichtbaar gemaakt
        allMenus[currentMenu].SetActive(true);

        // alle andere menus worden ontzichtbaar gemaakt.
        for (int i=0; i<allMenus.Length; i++)
        {
            if (i != currentMenu) allMenus[i].SetActive(false);
        }
    }

    public void DatumTijd()
    {
        DecideMenu(0);
        Extension();
        Back();
    }

    public void Weer()
    {
        DecideMenu(1);
        Extension();
        Back();
    }

    public void Zoeken()
    {
        DecideMenu(2);
        Extension();
        Back();
    }

    public void Uploaden()
    {
        DecideMenu(3);
        Extension();
        Back();
    }

    public void PlaatsenGebouw()
    {
        DecideMenu(4);
        Extension();
        Back();
    }

    public void Downloaden()
    {
        DecideMenu(5);
        Extension();
        Back();
    }

    public void Saven()
    {
        DecideMenu(6);
        Extension();
        Back();
    }

    public void Visualisatie()
    {
        DecideMenu(7);
        Extension();
        Back();
    }

    public void Opties()
    {
        DecideMenu(8);
        Extension();
        Back();
    }

    // kiest het huidige menu
    private void DecideMenu(int _currentMenu)
    {
        // het huidige menu verandert pas als het menu weer achter de opties is verdwenen
        if (menus.transform.position.x == 0) currentMenu = _currentMenu;
    }

    private void Extension()
    {
        // het menu wordt uitgeklapt
        if (menus.transform.position.x == 0)
        {
            extensionScript.extend = true;
        }
    }

    private void Back()
    {
        // het menu wordt ingeklapt
        if (menus.transform.position.x == extensionScript.extensionDistance)
        {
            extensionScript.goBack = true;
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
