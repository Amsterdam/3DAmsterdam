using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIToggle : MonoBehaviour
{
    protected Toggle toggle;

    protected virtual void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    protected virtual void OnEnable()
    {
        toggle.onValueChanged.AddListener(ToggleAction);
    }

    protected virtual void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(ToggleAction);
    }

    protected abstract void ToggleAction(bool active);
}
