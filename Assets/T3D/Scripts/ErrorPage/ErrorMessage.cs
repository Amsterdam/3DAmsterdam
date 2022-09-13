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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
}
