using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetaFeature : MonoBehaviour
{
    private void Awake()
    {
        ToggleActiveEvent.Subscribe(OnToggleActive);
    }

    private void OnToggleActive(object sender, ToggleActiveEvent.Args e)
    {
        var toggle = (bool)sender;
        gameObject.SetActive(toggle);
    }
}
