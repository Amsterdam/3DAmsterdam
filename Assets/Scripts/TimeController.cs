using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeController : MonoBehaviour
{
    public TextMeshProUGUI hours;
    public TextMeshProUGUI minutes;
    public TextMeshProUGUI time;
    public GameObject menu;

    private int _hours, _minutes;

    public Button upButtonHours, downButtonHours, upButtonMinutes, downButtonMinutes;

    private void Update()
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
    }

    public void MenuController()
    {
        if (menu.activeSelf) menu.SetActive(false);
        else
        {
            menu.SetActive(true);
        }
    }

    public void IncreaseMinutes()
    {
        if (_minutes >= 59) _minutes = 1;
        else
        {
            _minutes++;
        }
    }

    public void IncreaseHours()
    {
        if (_hours >= 23) _hours = 0;
        else
        {
            _hours++;
        }
    }

    public void DecreaseMinutes()
    {
        if (_minutes <= 0) _minutes = 59;
        else
        {
            _minutes--;
        }
    }

    public void DecreaseHours()
    {
        if (_hours <= 0) _hours = 23;
        else
        {
            _hours--;
        }
    }
}
