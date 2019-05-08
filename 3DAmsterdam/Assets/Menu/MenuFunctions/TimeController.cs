using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeController : MonoBehaviour
{
    public GameObject menuFunctions;
    private MenuFunctions _menuFunctions;

    private int hoursMax = 23, minutesMax = 59;
    private int currentHours, currentMinutes;

    private void Start()
    {
        _menuFunctions = menuFunctions.GetComponent<MenuFunctions>();

        currentHours = _menuFunctions._hours;
        currentMinutes = _menuFunctions._minutes;
    }

    public void ShowTimeInputField(GameObject inputField)
    {
        if (inputField.activeSelf)
        {
            inputField.SetActive(false);
        } else
        {
            inputField.SetActive(true);
        }
    }

    public void SelectHours(TMP_InputField inputHours)
    {
        int result = currentHours;
        string text = inputHours.text;

        int.TryParse(text, out result);

        if (result > hoursMax) result = hoursMax;
        if (result < 0) result = 0;

        _menuFunctions._hours = result;
    }

    public void SelectMinutes(TMP_InputField inputMinutes)
    {
        int result = currentMinutes;
        string text = inputMinutes.text;

        int.TryParse(text, out result);

        if (result > minutesMax) result = minutesMax;
        if (result < 0) result = 0;

        _menuFunctions._minutes = result;
    }

    public void ExitInputField(GameObject inputField)
    {
        inputField.SetActive(false);
    }
}
