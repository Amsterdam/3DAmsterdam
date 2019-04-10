using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;


public class EnterPosition : MonoBehaviour
{
    public InputField posX, posY, posZ;

    // object is moved to new location based on input
    public void NewPosition()
    {
        if (Regex.IsMatch(posX.text, @"^\d+$") && Regex.IsMatch(posY.text, @"^\d+$") &&
            Regex.IsMatch(posZ.text, @"^\d+$"))
        {
            transform.parent.parent.parent.position = new Vector3(float.Parse(posX.text), float.Parse(posY.text),
                                                                  float.Parse(posZ.text));
        }
    }
}
