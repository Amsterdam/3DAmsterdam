using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Traffic.VISSIM
{
    public class VissimConfigurationButton : MonoBehaviour
    {
        private VissimConfiguration vissimConfiguration = default;
        private Button btn = default;
        // Start is called before the first frame update
        void Start()
        {
            vissimConfiguration = FindObjectOfType<VissimConfiguration>();
            btn = GetComponent<Button>();
            btn.onClick.AddListener(vissimConfiguration.SetVehicles); // sets all vehicles and also starts the game
        }
    }
}