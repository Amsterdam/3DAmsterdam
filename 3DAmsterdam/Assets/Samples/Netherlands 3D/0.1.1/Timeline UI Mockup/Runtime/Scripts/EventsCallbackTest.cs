using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Netherlands3D.Timeline.Samples
{
    public class EventsCallbackTest : MonoBehaviour
    {
        public TimePeriod timePeriod;
        public GameObject cube;
        public TimelineUI timelineUI;
        public GameObject sphere;
        public TextMeshProUGUI eventStateField;

        private bool cubeActive = true;
        private bool sphereActive = true;

        private void OnEnable()
        {
            timePeriod.eventPressed.AddListener(ToggleCube);
            timelineUI.onCurrentDateChange.AddListener(OnDateChange);
            timelineUI.onCategoryToggle.AddListener(OnCategoryToggle);

            timePeriod.eventScreenEnter.AddListener(() => eventStateField.text = "ScreenEnter");
            timePeriod.eventScreenExit.AddListener(() => eventStateField.text = "ScreenExit");
            timePeriod.eventCurrentTimeEnter.AddListener(() => eventStateField.text = "CT Enter");
            timePeriod.eventCurrentTimeExit.AddListener(() => eventStateField.text = "CT Exit");
            timePeriod.eventLayerShow.AddListener(() => eventStateField.text = "Layer Show");
            timePeriod.eventLayerHide.AddListener(() => eventStateField.text = "Layer Hide");
            timePeriod.eventPressed.AddListener(() => eventStateField.text = "Pressed");
        }

        private void OnDisable()
        {
            timePeriod.eventPressed.RemoveListener(ToggleCube);
            timelineUI.onCurrentDateChange.RemoveListener(OnDateChange);
            timelineUI.onCategoryToggle.RemoveListener(OnCategoryToggle);

            timePeriod.eventScreenEnter.RemoveListener(() => eventStateField.text = "ScreenEnter");
            timePeriod.eventScreenExit.RemoveListener(() => eventStateField.text = "ScreenExit");
            timePeriod.eventCurrentTimeEnter.RemoveListener(() => eventStateField.text = "CT Enter");
            timePeriod.eventCurrentTimeExit.RemoveListener(() => eventStateField.text = "CT Exit");
            timePeriod.eventLayerShow.RemoveListener(() => eventStateField.text = "Layer Show");
            timePeriod.eventLayerHide.RemoveListener(() => eventStateField.text = "Layer Hide");
            timePeriod.eventPressed.RemoveListener(() => eventStateField.text = "Pressed");
        }

        public void ToggleCube()
        {
            cube.SetActive(!cube.activeSelf);
        }

        public void OnDateChange(DateTime date)
        {
            sphere.SetActive(sphereActive && TimeUnit.DateTimeInRange(date, new DateTime(2018, 1, 1), new DateTime(2024, 1, 1)));
        }

        public void OnCategoryToggle(string name, bool isActive)
        {
            if(name == "Bomen") cubeActive = isActive;
            cube.SetActive(cubeActive);
            if(name == "Auto's") sphereActive = isActive;
            sphere.SetActive(sphereActive && TimeUnit.DateTimeInRange(timelineUI.CurrentDate, new DateTime(2018, 1, 1), new DateTime(2024, 1, 1)));
        }
    }
}
