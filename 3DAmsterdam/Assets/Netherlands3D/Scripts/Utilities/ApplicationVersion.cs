using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationVersion : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = Application.version;
    }
}
