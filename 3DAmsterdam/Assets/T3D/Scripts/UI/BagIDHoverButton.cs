using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagIDHoverButton : InfoHoverButton
{
    private void Awake()
    {
        defaultPopupText = "Bag ID: " + T3DInit.HTMLData.BagId;
    }
}
