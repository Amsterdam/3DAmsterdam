using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableDisable : MonoBehaviour
{
    [SerializeField] private BoolEvent onEnabled;

    private void OnEnable()
    {
        onEnabled.started.Invoke(true);
    }

    private void OnDisable()
    {
        onEnabled.started.Invoke(false);
    }
}
