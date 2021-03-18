using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Netherlands3D.Sun
{
    [ExecuteInEditMode]
    public class SunTime : MonoBehaviour
    {
        [SerializeField]
        private float longitude;

        [SerializeField]
        private float latitude;

        [SerializeField]
        [Range(0, 24)]
        private int hour;

        [SerializeField]
        [Range(0, 60)]
        private int minutes;

        private DateTime time;
        private Light sunLight;

        [SerializeField]
        private float timeSpeed = 1;

        [SerializeField]
        private int frameSteps = 1;
        private int frameStep;

        public void SetTime(DateTime time) {
            this.time = time;
            OnValidate();
        }

        public void SetTime(int hour, int minutes) {
            this.hour = hour;
            this.minutes = minutes;
            OnValidate();
        }

        public void SetLocation(float longitude, float latitude){
          this.longitude = longitude;
          this.latitude = latitude;
        }

        public void SetUpdateSteps(int i) {
            frameSteps = i;
        }

        public void SetTimeSpeed(float speed) {
            timeSpeed = speed;
        }

        private void Start()
        {
            sunLight = GetComponent<Light>();
            time = DateTime.Now;
            hour = time.Hour;
            minutes = time.Minute;
        }

        private void OnValidate()
        {
            time = DateTime.Now.Date + new TimeSpan(hour, minutes, 0);
        }

        private void Update()
        {
            time = time.AddSeconds(timeSpeed * Time.deltaTime);
            if (frameStep==0) {
                SetPosition();
            }
            frameStep = (frameStep + 1) % frameSteps;
		}

        void SetPosition()
        {
            Vector3 angles = new Vector3();
            double alt;
            double azi;
            SunPosition.CalculateSunPosition(time, (double)latitude, (double)longitude, out azi, out alt);
            angles.x = (float)alt * Mathf.Rad2Deg;
            angles.y = (float)azi * Mathf.Rad2Deg;
            //UnityEngine.Debug.Log(angles);
            transform.localRotation = Quaternion.Euler(angles);
            sunLight.intensity = Mathf.InverseLerp(-12, 0, angles.x);
        }        
    }
}