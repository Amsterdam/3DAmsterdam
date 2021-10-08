using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonVisualiser : MonoBehaviour
{
    [SerializeField]
    private StringEvent receivePointsAsStringEvent;

    void Start()
    {
        receivePointsAsStringEvent.stringEvent.AddListener(ReceivePointsString);
    }

    // Update is called once per frame
    void ReceivePointsString(string pointsString)
    {
        
    }
}
