using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = ErrorService.ErrorMessage;
    }
}
