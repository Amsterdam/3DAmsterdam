using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;

public class TimelineSlider : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private GameObject dateTextPrefab;
    [SerializeField] private Slider slider;
    [SerializeField] private Transform dateContainer;
    [SerializeField] private TimelineFormat format = TimelineFormat.Year;
    [SerializeField] private float distanceIncrease = 120;

    [Header("Controls")]
    [SerializeField] private TMP_InputField dayText;
    [SerializeField] private TMP_InputField monthText;
    [SerializeField] private TMP_InputField yearText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private Toggle playButton;

    [Header("Event listner")]
    [SerializeField] private UnityEvent<TimelineElement> setTimelineDataEvent;

    [Header("Event invoker")]
    [SerializeField] private UnityEvent<DateTime> changeTimelineDateEvent;



    private int playSpeed = 1;
    private DateTime startingDate;
    private DateTime endDate;
    private int amountInstances = 0;
    private List<DateTime> allDatesBetweenStartAndEnd;

    void Awake()
    {
        setTimelineDataEvent.AddListener(Setup);
        //startingDate = DateTime.Now;
        //endDate = startingDate.AddYears(20).AddMonths(4);

        //Setup(this.format);
    }

    private void Setup(TimelineElement timelineElement)
    {
        startingDate = timelineElement.StartDate;
        endDate = timelineElement.EndDate;

        Setup(this.format);
    }

    private void Setup(TimelineFormat format)
    {
        SetAllDates();

        List<DateTime> allDates = GetAllGeneratedDates(format);
        foreach (DateTime date in allDates)
        {
            string text = date.ToString("yyyy");
            if (format == TimelineFormat.Month) text = date.ToString("yyyy/MM");

            Instantiate(dateTextPrefab, dateContainer).GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

        //transform.parent.transform.GetComponent<RectTransform>().rect.width
        int minimumValue = Screen.width + (int) transform.transform.GetComponent<RectTransform>().sizeDelta.x; //Idealy is the size of the conatiner
        if (distanceIncrease * (allDates.Count - 1) < minimumValue)
        {
            distanceIncrease = minimumValue / (allDates.Count - 1);
        }

        //This will determine the scale between the times
        int edgeSpacing = 64;
        float horizontalSpacing = (((int)distanceIncrease * (allDates.Count - 1)) - edgeSpacing) / (allDates.Count - 1);

        dateContainer.GetComponent<HorizontalLayoutGroup>().spacing = horizontalSpacing;
        slider.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(distanceIncrease * (allDates.Count - 1), slider.gameObject.GetComponent<RectTransform>().sizeDelta.y);
        slider.minValue = 0;
        slider.maxValue = allDatesBetweenStartAndEnd.Count - 1;

        //Set initial posution
        int index = (int)allDatesBetweenStartAndEnd.Count / 2;
        slider.value = index;
        SetDateToTextfield(allDatesBetweenStartAndEnd[index]);


    }

    private void SetAllDates()
    {
        allDatesBetweenStartAndEnd = new List<DateTime>();
        DateTime iterator = new DateTime(startingDate.Year, startingDate.Month, 1);
        DateTime limit = endDate;

        while (iterator <= limit)
        {
            allDatesBetweenStartAndEnd.Add(new DateTime(iterator.Year, iterator.Month, 1));
            iterator = iterator.AddDays(1);
        }
    }

    private List<DateTime> GetAllGeneratedDates(TimelineFormat format) {
        List<DateTime> dates = new List<DateTime>();

        switch (format)
        {
            case TimelineFormat.Year:
                amountInstances = endDate.Year - startingDate.Year;
                for (int i = 0; i <= amountInstances; i++)
                {
                    dates.Add(new DateTime(startingDate.Year + i, 1, 1));
                }

                break;
            case TimelineFormat.Month:
                DateTime iterator = new DateTime(startingDate.Year, startingDate.Month, 1);
                DateTime limit = endDate;

                while (iterator <= limit)
                {
                    dates.Add(new DateTime(iterator.Year, iterator.Month, 1));
                    iterator = iterator.AddMonths(1);
                }

                break;
        }

        var additionalDate = dates[dates.Count - 1];
        additionalDate = (format == TimelineFormat.Year) ? additionalDate.AddYears(1) : additionalDate.AddMonths(1);
        dates.Add(additionalDate);

        return dates;
    }

    public void SetTextfield(float amount)
    {
        DateTime date = ConvertIntToDatetime((int) Math.Floor((decimal) amount));

        changeTimelineDateEvent.Invoke(date);       //EVENT INVOKE
        SetDateToTextfield(date); 
    }

    public void SetDateToTextfield(DateTime date)
    {
        Debug.Log($"date: {date.ToString("yyyy MM dd")}");

        dayText.text = date.ToString("dd");
        monthText.text = date.ToString("MM");
        yearText.text = date.ToString("yyyy");

    }


    public void SetToSlider()
    {
        DateTime newDate = new DateTime(
            SetSpecificDate(yearText, startingDate.Year),
            SetSpecificDate(monthText, startingDate.Month),
            SetSpecificDate(dayText, startingDate.Day));


        if (newDate < startingDate || newDate > endDate)
        {
            newDate = startingDate;
        }

        int value = ConvertDateTimeToInt(newDate);

        slider.value = value != -1 ? value : 0;
    }

    private int SetSpecificDate(TMP_InputField textMesh, int defaultValue)
    {
        if (int.TryParse(textMesh.text, out int result))
        {
            defaultValue = result;
        }
        {
            textMesh.text = defaultValue.ToString("00");
        }

        return defaultValue;
    }

    private int ConvertDateTimeToInt(DateTime date)
    { 
        return allDatesBetweenStartAndEnd.IndexOf(date);
    }
    private DateTime ConvertIntToDatetime(int sliderAmount)
    {
        return allDatesBetweenStartAndEnd[sliderAmount];
    }

    public void StartPlay()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        int multiplier = 2;

        while (slider.value < slider.maxValue && playButton.isOn)
        {
            slider.value += playSpeed * multiplier;
            yield return null;
        }
    }

    public void IncrementLoopMultipler()
    {
        playSpeed++;
        if (playSpeed == 5) playSpeed = 1;

        multiplierText.text = $"{playSpeed.ToString()}x";
    }

}


public enum TimelineFormat
{
    Year,
    Month
}