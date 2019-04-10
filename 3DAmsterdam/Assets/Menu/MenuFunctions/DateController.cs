using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DateController : MonoBehaviour
{
    public GameObject menu;
    public GameObject twentyNine, thirty, thirtyOne;

    public TextMeshProUGUI monthYear;
    private TextMeshProUGUI _monthyear;

    public TextMeshProUGUI date;
    private TextMeshProUGUI _date;
    
    private string[] months = new string[] {"Januari", "Februari", "Maart", "April", "Mei", "Juni",
                                            "Juli", "Augustus", "September", "Oktober", "November", "December" };

    private int currentDay, currentMonth, currentYear;

    private void Start()
    {
        currentDay = System.DateTime.Now.Day;
        currentMonth = System.DateTime.Now.Month;
        currentYear = System.DateTime.Now.Year;

        _monthyear = monthYear.GetComponent<TextMeshProUGUI>();
        _date = date.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _monthyear.text = months[currentMonth - 1] + " " + currentYear;
        _date.text = currentDay + "-" + currentMonth + "-" + currentYear;

        DaySelection();
    }

    private void DaySelection()
    {
        if (currentYear % 4 == 0 && currentMonth != 2)
        {
            twentyNine.SetActive(true);
            thirty.SetActive(true);
        }

        switch (currentMonth)
        {
            case 1:
                thirtyOne.SetActive(true);
                break;
            case 2:
                if (currentYear % 4 == 0)
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
    }


    public void RightButton()
    {
        if (currentMonth < 12)
        {
            currentMonth++;
        } else
        {
            currentYear++;
            currentMonth = 1;
        }
    }

    public void LeftButton()
    {
        if (currentMonth > 1)
        {
            currentMonth--;
        }
        else
        {
            currentYear--;
            currentMonth = 12;
        }
    }

    public void MenuController()
    {
        if (menu.activeSelf) menu.SetActive(false);
        else
        {
            menu.SetActive(true);
        }
    }

    public void SelectDay()
    {
        currentDay = int.Parse(EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
    }
}
