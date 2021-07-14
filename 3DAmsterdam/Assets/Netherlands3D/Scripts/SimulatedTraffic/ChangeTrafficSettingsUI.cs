using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Traffic
{
    public class ChangeTrafficSettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider speedSlider = default;
        [SerializeField] private Text speedText = default;

        [SerializeField] private Toggle startCarToggle = default;

        [SerializeField] private TrafficSimulator trafficSimulator;

        // Start is called before the first frame update
        void Start()
        {
            speedSlider.onValueChanged.AddListener(delegate { UpdateVehicleSpeed(); });
            speedSlider.value = trafficSimulator.vehicleSpeed;
            startCarToggle.onValueChanged.AddListener(delegate { StartCars(); });
        }

        /// <summary>
        /// Changes the vehicle speed through an UI element
        /// </summary>
        public void UpdateVehicleSpeed()
        {
            speedText.text = speedSlider.value.ToString();
            trafficSimulator.vehicleSpeed = Mathf.RoundToInt(speedSlider.value);
        }

        /// <summary>
        /// Start/Stops all vehicles through an UI element
        /// </summary>
        public void StartCars()
        {
            trafficSimulator.StartSimulation(startCarToggle.isOn);
        }
    }
}