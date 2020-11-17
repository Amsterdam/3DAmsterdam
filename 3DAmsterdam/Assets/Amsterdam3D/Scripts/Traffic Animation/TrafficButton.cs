using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrafficButton : MonoBehaviour
{
    private Button carButton = default;
    // Start is called before the first frame update
    void Start()
    {
        carButton = GetComponent<Button>();
        //carButton.onClick.AddListener(TrafficSimulator.Instance.StartSimulation);
    }

}
