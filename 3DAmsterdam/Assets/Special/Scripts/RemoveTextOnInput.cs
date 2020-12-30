using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveTextOnInput : MonoBehaviour
{
    [SerializeField]
    KeyCode[] keysThatHideText;

    void Update()
    {
        foreach(KeyCode key in keysThatHideText)
        {
            if(Input.GetKeyDown(key))
            {
                this.gameObject.SetActive(false);
			}
		}
    }
}
