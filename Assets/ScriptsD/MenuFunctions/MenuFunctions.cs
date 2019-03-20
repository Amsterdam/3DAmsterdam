using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuFunctions : MonoBehaviour
{
    public GameObject[] allMenus;
    public Button[] buttons;
    private int currentMenu;
    private int noMenu = 10;

    public Toggle OptionsToggleOne, OptionsToggleTwo, PlaceBuildingToggle;
    public GameObject coordinatesPanel, miniMap, placeBuildingMenu;
    public Slider heightSlider, widthSlider, lengthSlider;
    public TextMeshProUGUI heightValue, widthValue, lengthValue;
    public Image cubeImage;
    public Sprite spriteHeight, spriteWidth, spriteLength, spriteRegular;

    private TextMeshProUGUI _heightValue, _widthValue, _lengthValue;

    private float scaleFactor = 50f;

    private void Start()
    {
        currentMenu = noMenu;

        _heightValue = heightValue.GetComponent<TextMeshProUGUI>();
        _widthValue = widthValue.GetComponent<TextMeshProUGUI>();
        _lengthValue = lengthValue.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // het huidige menu wordt zichtbaar gemaakt
        if (currentMenu != noMenu)
        {
            allMenus[currentMenu].SetActive(true);
        }
        
        // alle andere menus worden ontzichtbaar gemaakt.
        for (int i = 0; i < allMenus.Length; i++)
        {
            if (i != currentMenu) allMenus[i].SetActive(false);
        }

        ToggleMapAndCoordinates();
        changePlaceBuildingValues();
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

    public void Exit()
    {
        currentMenu = noMenu;
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
    public void ToggleMapAndCoordinates()
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

    public void TogglePlaceBuilding()
    {
        if (PlaceBuildingToggle.GetComponent<Toggle>().isOn)
        {
            placeBuildingMenu.SetActive(true);
        }
        else
        {
            placeBuildingMenu.SetActive(false);
        }
    }

    private void changePlaceBuildingValues()
    {
        _heightValue.text = ((int)(heightSlider.value * scaleFactor)).ToString() + "m";
        _widthValue.text = ((int)(widthSlider.value * scaleFactor)).ToString() + "m";
        _lengthValue.text = ((int)(lengthSlider.value * scaleFactor)).ToString() + "m";

        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (heightSlider.name == EventSystem.current.currentSelectedGameObject.name)
            {
                cubeImage.GetComponent<Image>().sprite = spriteHeight;
            } else if (widthSlider.name == EventSystem.current.currentSelectedGameObject.name)
            {
                cubeImage.GetComponent<Image>().sprite = spriteWidth;
            } else
            {
                cubeImage.GetComponent<Image>().sprite = spriteLength;
            } 
        } else
        {
            cubeImage.GetComponent<Image>().sprite = spriteRegular;
        }
    }
}
