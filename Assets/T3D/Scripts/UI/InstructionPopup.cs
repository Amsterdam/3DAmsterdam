using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class InstructionPopup : MonoBehaviour
{
    private void Start()
    {
        if (SessionSaver.LoadPreviousSession)
        {
            ClosePopup();
        }
    }

    //also used by close button in the inspector
    public void ClosePopup()
    {
        Destroy(gameObject);
    }
}
