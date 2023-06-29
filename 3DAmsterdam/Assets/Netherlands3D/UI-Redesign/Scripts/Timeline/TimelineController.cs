using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimelineController : MonoBehaviour
{

    [Header("Invoker")]
    [SerializeField] private UnityEvent setTimelineElement;
    [SerializeField] private UnityEvent<TimelineElement> setTimelineElementData;
    [SerializeField] private UnityEvent<TimelineElement> fillTimeline;

    private TimelineElement timelineElement;


    //"setTimelineElementData" should be invoked where the data is generated. Not here
    //for demontartion purposes, it is also being sent here
    //[SerializeField] private UnityEvent<TimelineElement> setTimelineElementData;

    void Start()
    {
        Debug.Log("!! Awake");

        setTimelineElement.Invoke();
    }

    public void OnCheckPressed()
    {
        fillTimeline.Invoke(timelineElement);

        Debug.Log($"!! OnCheckPressed {timelineElement.EndDate}");
    }

    public void SetData(TimelineElement timelineElement)
    {
        this.timelineElement = timelineElement;

        //Sets the legend of the timeline
        transform.GetComponent<DataAccordionController>()?.SetUp(timelineElement.Legend);

        //Sets the actual timeline
        fillTimeline.Invoke(timelineElement);

        Debug.Log("!! SetData(TimelineElement timelineElement)");
    }


    //this should be in the controller/data layer
    //This is called when the "setTimelineElement" is listened to 
    public void SendTimelineData()
    {
        Debug.Log("!! SendTimelineData");

        //Example
        timelineElement = new TimelineElement(
            "Example Timeline",
            new List<ColorLegendItem>() {
                new ColorLegendItem("Legend 1", Color.green),
                new ColorLegendItem("Legend 2", Color.blue),
                new ColorLegendItem("Legend 3", Color.red)
            },
            DateTime.Now,
            DateTime.Now.AddYears(20));

        setTimelineElementData.Invoke(timelineElement);
    }

    public void SendTimeline2Data()
    {
        Debug.Log("!! SendTimelineData2");

        //Example
        timelineElement = new TimelineElement(
            "Example Timeline",
            new List<ColorLegendItem>() {
                new ColorLegendItem("Legend 1", Color.green),
                new ColorLegendItem("Legend 2", Color.blue),
                new ColorLegendItem("Legend 3", Color.red),
                new ColorLegendItem("Legend 4", Color.yellow),
                new ColorLegendItem("Legend 5", Color.black)

            },
            DateTime.Now,
            DateTime.Now.AddYears(5));

        setTimelineElementData.Invoke(timelineElement);
    }

}
