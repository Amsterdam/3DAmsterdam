using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagIDHoverButton : InfoHoverButton
{
    private void Awake()
    {
        defaultPopupText = "Bag ID: " + ServiceLocator.GetService<T3DInit>().HTMLData.BagId;
    }
}
