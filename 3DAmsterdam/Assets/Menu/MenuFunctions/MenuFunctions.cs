using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuFunctions : MonoBehaviour
{
    [Header("Algemeen")]
    public GameObject[] allMenus;
    public Button[] buttons;
    public GameObject coordinatesPanel, miniMap, zoomButtonPlus, zoomButtonMinus, compass, streetView, godView, manager;

    [Header("Tijd en Weer")]
    public GameObject[] weatherOptions;
    public GameObject timeMenu, dateMenu, twentyNine, thirty, thirtyOne;
    public Button upButtonHours, downButtonHours, upButtonMinutes, downButtonMinutes;
    public RectTransform positioning;
    public TextMeshProUGUI hours, minutes, time, monthYear, date;
    private bool huidigSelected = false;

    [Header("Bouwen")]
    public GameObject[] buildingOptions;
    public GameObject placeBuildingMenu, uploadBuildingMenu, createBuilding, uploadBuilding;
    public Slider heightSlider, widthSlider, lengthSlider, rotationSlider;
    public Image cubeImage;
    public Sprite spriteHeight, spriteWidth, spriteLength, spriteRegular, spriteRightArrow, spriteDownArrow;
    public Button placeBuildingButton, uploadBuildingButton;
    public RectTransform positioningGebouwen;
    public TextMeshProUGUI heightValue, widthValue, lengthValue, rotationValue;

    [Header("Opties")]
    public Toggle[] optionsToggles;

    private ModeManager modeManager;

    private TextMeshProUGUI _monthyear, _date, _heightValue, _widthValue, _lengthValue, _rotationValue;

    private Vector3 startPosMenuCreateBuilding, startPosMap;

    private int noMenu = 10, minBarrierGebouwen = 0, maxBarrierGebouwen = 2,
                buildingMenuDecider = 1, uploadMenuDecider = 1;

    [HideInInspector]
    public int currentMenu;

    [HideInInspector]
    public int _hours, _minutes, _currentDay, _currentMonth, _currentYear, _currentWeer;


    private float scaleFactor = 100f, moveFactor = 55f;

    private string[] months = new string[] {"Januari", "Februari", "Maart", "April", "Mei", "Juni",
                                            "Juli", "Augustus", "September", "Oktober", "November", "December" };

    private void Start()
    {
        currentMenu = noMenu;

        _heightValue = heightValue.GetComponent<TextMeshProUGUI>();
        _widthValue = widthValue.GetComponent<TextMeshProUGUI>();
        _lengthValue = lengthValue.GetComponent<TextMeshProUGUI>();
        _rotationValue = rotationValue.GetComponent<TextMeshProUGUI>();
        _monthyear = monthYear.GetComponent<TextMeshProUGUI>();
        _date = date.GetComponent<TextMeshProUGUI>();

        _currentDay = System.DateTime.Now.Day;
        _currentMonth = System.DateTime.Now.Month;
        _currentYear = System.DateTime.Now.Year;
        _hours = 12;

        startPosMenuCreateBuilding = createBuilding.transform.localPosition;
        startPosMap = miniMap.transform.localPosition;

        _currentWeer = weatherOptions.Length / 2;

        modeManager = manager.GetComponent<ModeManager>();
    }

    private void Update()
    {
        ManageMenus();

        // tijd en weer menu
        ShowWeatherOptions();
        TimeManager();
        DateManager();

        // opties menu
        TogglingItems();

        // plaats gebouw menu
        ShowBuildingOptions();
        changePlaceBuildingValues();

        // minimap
        MapPositioning();
    }

    // beheert alle menus
    private void ManageMenus()
    {
        // het huidige menu wordt zichtbaar gemaakt
        if (currentMenu != noMenu)
        {
            var menu = allMenus[currentMenu];

            menu.SetActive(true);

            menu.transform.localPosition = new Vector3(Mathf.Lerp(menu.transform.localPosition.x, 187f, 6f * Time.deltaTime), 0, 0);
        }

        // alle andere menus worden ontzichtbaar gemaakt.
        for (int i = 0; i < allMenus.Length; i++)
        {
            if (i != currentMenu)
            {
                allMenus[i].transform.localPosition = Vector3.zero;
                allMenus[i].SetActive(false);
            }
        }
    }

    #region MainMenu
    // een menu wordt geselecteerd
    public void SelectMenu(int menuNumber)
    {
        // voor een array is de index een lager (begint bij 0)
        int _menuNumber = menuNumber - 1;

        currentMenu = _menuNumber;

        GameObject menu = allMenus[_menuNumber];
        Button button = buttons[_menuNumber];
        ColorBlock colors = button.colors;

        // als het menu actief is wordt de knop niet meer highlighted, als het menu niet actief is wordt de knop gehighlight
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

    public void Exit()
    {
        currentMenu = noMenu;
    }
    #endregion


    #region TijdWeerMenu
    // weerselectie naar rechts
    public void WeerRightButton()
    {
        if (_currentWeer > 0)
        {
            positioning.position += new Vector3(moveFactor, 0, 0);
            _currentWeer--;
        }
    }

    // weerselectie naar links
    public void WeerLeftButton()
    {
        if (_currentWeer < 6)
        {
            positioning.position -= new Vector3(moveFactor, 0, 0);
            _currentWeer++;
        }
    }

    // zichtbaarheid van weeropties 
    private void ShowWeatherOptions()
    {
        for (int i = 0; i < weatherOptions.Length; i++)
        {
            if (i == _currentWeer)
            {
                weatherOptions[i].GetComponent<Button>().enabled = true;
                weatherOptions[i].GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                weatherOptions[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                weatherOptions[i].transform.GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                weatherOptions[i].GetComponent<Button>().enabled = false;
                weatherOptions[i].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                weatherOptions[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.3f);
                weatherOptions[i].transform.GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.3f);
            }

            switch (_currentWeer)
            {
                case 0:
                    if (i >= 3 && i <= 6)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
                case 1:
                    if (i >= 4 && i <= 6)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
                case 2:
                    if (i == 5 || i == 6)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
                case 3:
                    if (i == 0 || i == 6)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
                case 4:
                    if (i == 0 || i == 1)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
                case 5:
                    if (i >= 0 && i <= 2)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
                case 6:
                    if (i >= 0 && i <= 3)
                    {
                        weatherOptions[i].SetActive(false);
                    }
                    else
                    {
                        weatherOptions[i].SetActive(true);
                    }
                    break;
            }
        }   
    }

    // zichtbaarheid van tijd menu
    public void TimeMenuController()
    {
        if (timeMenu.activeSelf) timeMenu.SetActive(false);
        else
        {
            timeMenu.SetActive(true);
        }
    }

    // verhoogt de minuten
    public void IncreaseMinutes()
    {
        if (_minutes >= 59) _minutes = 1;
        else
        {
            _minutes++;
        }
    }

    // verhoogt de uren
    public void IncreaseHours()
    {
        if (_hours >= 23) _hours = 0;
        else
        {
            _hours++;
        }
    }

    // verlaagt de minuten
    public void DecreaseMinutes()
    {
        if (_minutes <= 0) _minutes = 59;
        else
        {
            _minutes--;
        }
    }

    // verlaagt de uren
    public void DecreaseHours()
    {
        if (_hours <= 0) _hours = 23;
        else
        {
            _hours--;
        }
    }

    // vertoont de tijd juist in beeld
    private void TimeManager()
    {
        if (_hours < 10)
        {
            hours.GetComponent<TextMeshProUGUI>().text = "0" + _hours.ToString();
            time.GetComponent<TextMeshProUGUI>().text = "0" + _hours.ToString() + ":" + _minutes.ToString();
        }
        else
        {
            hours.GetComponent<TextMeshProUGUI>().text = _hours.ToString();
            time.GetComponent<TextMeshProUGUI>().text = _hours.ToString() + ":" + _minutes.ToString();
        }

        if (_minutes < 10)
        {
            minutes.GetComponent<TextMeshProUGUI>().text = "0" + _minutes.ToString();
            time.GetComponent<TextMeshProUGUI>().text = _hours.ToString() + ":" + "0" + _minutes.ToString();
        }
        else
        {
            minutes.GetComponent<TextMeshProUGUI>().text = _minutes.ToString();
            time.GetComponent<TextMeshProUGUI>().text = _hours.ToString() + ":" + _minutes.ToString();
        }


        if (huidigSelected)
        {
            _hours = System.DateTime.Now.Hour;
            _minutes = System.DateTime.Now.Minute;
        }
    }

    // vertoont de datum juist in beeld
    private void DateManager()
    {
        _monthyear.text = months[_currentMonth - 1] + " " + _currentYear;
        _date.text = _currentDay + "-" + _currentMonth + "-" + _currentYear;

        if (_currentYear % 4 == 0 && _currentMonth != 2)
        {
            twentyNine.SetActive(true);
            thirty.SetActive(true);
        }

        switch (_currentMonth)
        {
            case 1:
                thirtyOne.SetActive(true);
                break;
            case 2:
                if (_currentYear % 4 == 0)
                {
                    twentyNine.SetActive(false);
                    thirty.SetActive(false);
                }

                thirtyOne.SetActive(false);
                break;
            case 3:
                thirtyOne.SetActive(true);
                break;
            case 4:
                thirtyOne.SetActive(false);
                break;
            case 5:
                thirtyOne.SetActive(true);
                break;
            case 6:
                thirtyOne.SetActive(false);
                break;
            case 7:
                thirtyOne.SetActive(true);
                break;
            case 8:
                thirtyOne.SetActive(true);
                break;
            case 9:
                thirtyOne.SetActive(false);
                break;
            case 10:
                thirtyOne.SetActive(true);
                break;
            case 11:
                thirtyOne.SetActive(false);
                break;
            case 12:
                thirtyOne.SetActive(true);
                break;
        }

        if (huidigSelected)
        {
            _currentDay = System.DateTime.Now.Day;
            _currentMonth = System.DateTime.Now.Month;
            _currentYear = System.DateTime.Now.Year;
        }
    }

    // rechterknop om de maand te selecteren
    public void DateRightButton()
    {
        if (_currentMonth < 12)
        {
            _currentMonth++;
        }
        else
        {
            _currentYear++;
            _currentMonth = 1;
        }
    }

    // linkerknop om de maand te selecteren
    public void DateLeftButton()
    {
        if (_currentMonth > 1)
        {
            _currentMonth--;
        }
        else
        {
            _currentYear--;
            _currentMonth = 12;
        }
    }

    // zichtbaarheid van datum menu
    public void DateMenuController()
    {
        if (dateMenu.activeSelf) dateMenu.SetActive(false);
        else
        {
            dateMenu.SetActive(true);
        }
    }

    // selecteert de juiste dag
    public void SelectDay()
    {
        _currentDay = int.Parse(EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
        dateMenu.SetActive(false);
    }

    public void HuidigMoment()
    {
        if (huidigSelected == false)
        {
            huidigSelected = true;
        } else
        {
            huidigSelected = false;
        }
    }
    #endregion


    #region PlaatsGebouwMenu
    //// methode om submenu(gebouw plaatsen) aan/uit te zetten
    //public void TogglePlaceBuilding()
    //{
    //    buildingMenuDecider *= -1;

    //    if (buildingMenuDecider == 1)
    //    {
    //        placeBuildingMenu.SetActive(false);
    //        placeBuildingButton.GetComponent<Image>().sprite = spriteRightArrow;
    //    }
    //    else
    //    {
    //        placeBuildingMenu.SetActive(true);
    //        placeBuildingButton.GetComponent<Image>().sprite = spriteDownArrow;

    //        if (uploadBuildingMenu.activeSelf)
    //        {
    //            uploadMenuDecider *= -1;
    //            uploadBuildingMenu.SetActive(false);
    //            uploadBuildingButton.GetComponent<Image>().sprite = spriteRightArrow;
    //        }
    //    }
    //}

    //public void ToggleUploadBuilding()
    //{
    //    uploadMenuDecider *= -1;

    //    if (uploadMenuDecider == 1)
    //    {
    //        uploadBuildingMenu.SetActive(false);
    //        uploadBuildingButton.GetComponent<Image>().sprite = spriteRightArrow;

    //        createBuilding.transform.localPosition = startPosMenuCreateBuilding;
    //    }
    //    else
    //    {
    //        uploadBuildingMenu.SetActive(true);
    //        uploadBuildingButton.GetComponent<Image>().sprite = spriteDownArrow;

    //        createBuilding.transform.localPosition = new Vector3(createBuilding.transform.localPosition.x,
    //                                                        uploadBuildingMenu.transform.localPosition.y - (uploadBuildingMenu.GetComponent<RectTransform>().sizeDelta.y / 2 + 10),
    //                                                        createBuilding.transform.localPosition.z);

    //        if (placeBuildingMenu.activeSelf)
    //        {
    //            buildingMenuDecider *= -1;
    //            placeBuildingMenu.SetActive(false);
    //            placeBuildingButton.GetComponent<Image>().sprite = spriteRightArrow;
    //        }
    //    }
    //}

    // methode om kubus van kleur te laten veranderen zodat zichtbaar wordt of hoogte, lengte of breedte wordt aangepast
    private void changePlaceBuildingValues()
    {
        _heightValue.text = ((int)(heightSlider.value * scaleFactor)).ToString() + "m";
        _widthValue.text = ((int)(widthSlider.value * scaleFactor)).ToString() + "m";
        _lengthValue.text = ((int)(lengthSlider.value * scaleFactor)).ToString() + "m";
        _rotationValue.text = ((int)(rotationSlider.value * 360f)).ToString() + "°";


        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (heightSlider.name == EventSystem.current.currentSelectedGameObject.name)
            {
                cubeImage.GetComponent<Image>().sprite = spriteHeight;
            }
            else if (widthSlider.name == EventSystem.current.currentSelectedGameObject.name)
            {
                cubeImage.GetComponent<Image>().sprite = spriteWidth;
            }
            else
            {
                cubeImage.GetComponent<Image>().sprite = spriteLength;
            }
        }
        else
        {
            cubeImage.GetComponent<Image>().sprite = spriteRegular;
        }
    }

    // weerselectie naar rechts
    public void GebouwenRightButton()
    {
        if (positioningGebouwen.localPosition.x > -((buildingOptions.Length - 3) * moveFactor))
        {
            positioningGebouwen.position -= new Vector3(moveFactor, 0, 0);

            minBarrierGebouwen++;
            maxBarrierGebouwen++;
        }
    }

    // weerselectie naar links
    public void GebouwenLeftButton()
    {
        if (positioningGebouwen.localPosition.x < 0)
        {
            positioningGebouwen.position += new Vector3(moveFactor, 0, 0);

            minBarrierGebouwen--;
            maxBarrierGebouwen--;
        }
    }

    // zichtbaarheid van weeropties 
    private void ShowBuildingOptions()
    {
        for (int i = 0; i < buildingOptions.Length; i++)
        {
            if (i >= minBarrierGebouwen && i <= maxBarrierGebouwen)
            {
                buildingOptions[i].SetActive(true);
            }
            else
            {
                buildingOptions[i].SetActive(false);
            }
        }
    }
    #endregion


    #region OptiesMenu
    // zet minimap en coordinates panelen uit/aan onder opties menu
    public void TogglingItems()
    {
        if (optionsToggles[0].GetComponent<Toggle>().isOn)
        {
            miniMap.SetActive(true);
        } else
        {
            miniMap.SetActive(false);
        }

        if (optionsToggles[1].GetComponent<Toggle>().isOn)
        {
            coordinatesPanel.SetActive(true);
        } else
        {
            coordinatesPanel.SetActive(false);
        }

        if (optionsToggles[2].GetComponent<Toggle>().isOn)
        {
            compass.SetActive(true);
        } else
        {
            compass.SetActive(false);
        }

        if (optionsToggles[3].GetComponent<Toggle>().isOn)
        {
            if (manager.GetComponent<ModeManager>().mode != -1)
            {
                zoomButtonPlus.SetActive(true);
                zoomButtonMinus.SetActive(true);
            }
        } else
        {
            zoomButtonPlus.SetActive(false);
            zoomButtonMinus.SetActive(false);
        }

        if (optionsToggles[4].GetComponent<Toggle>().isOn)
        {
            if (manager.GetComponent<ModeManager>().mode != -1) streetView.SetActive(true);
        } else
        {
            streetView.SetActive(false);
        }
    }
    #endregion


    #region Minimap
    private void MapPositioning()
    {
        if (modeManager.mode == 1)
        {
            if (!(zoomButtonMinus.activeSelf) && !(zoomButtonPlus.activeSelf) && !(streetView.activeSelf))
            {
                miniMap.transform.localPosition = new Vector3(startPosMap.x + 70f, startPosMap.y, startPosMap.z);
            }
            else
            {
                miniMap.transform.localPosition = startPosMap;
            }
        } else
        {
            if (!(godView.activeSelf))
            {
                miniMap.transform.localPosition = new Vector3(startPosMap.x + 70f, startPosMap.y, startPosMap.z);
            }
            else
            {
                miniMap.transform.localPosition = startPosMap;
            }
        }
    }
    #endregion
}