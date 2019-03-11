using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeController : MonoBehaviour
{
    public Scrollbar scrollBar;

    public TextMeshProUGUI timeText;
    private TextMeshProUGUI _timeText;

    private void Start()
    {
        _timeText = timeText.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (scrollBar.value == 0) _timeText.text = "00:00";
        else if (scrollBar.value == (float) 1 / 8) _timeText.text = "03:00";
        else if (scrollBar.value == (float) 2 / 8) _timeText.text = "06:00";
        else if (scrollBar.value == (float) 3 / 8) _timeText.text = "09:00";
        else if (scrollBar.value == (float) 4 / 8) _timeText.text = "12:00";
        else if (scrollBar.value == (float) 5 / 8) _timeText.text = "15:00";
        else if (scrollBar.value == (float) 6 / 8) _timeText.text = "18:00";
        else if (scrollBar.value == (float) 7 / 8) _timeText.text = "21:00";
        else
        {
            _timeText.text = "23:59";
        }
    }
}
