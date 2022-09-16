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
    public void SetVisible(bool visible)
    {
        //var drawChange = ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel;
        if (visible == gameObject.activeInHierarchy)
            return;

        gameObject.SetActive(visible);
        if (!visible)
        {
            var sd = transform.parent.GetComponent<RectTransform>().sizeDelta;
            //sizeDelta is a value type so cannot directly assign it
            transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(sd.x, sd.y - GetComponent<RectTransform>().sizeDelta.y);
        }
        else
        {
            var sd = transform.parent.GetComponent<RectTransform>().sizeDelta;
            //sizeDelta is a value type so cannot directly assign it
            transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(sd.x, sd.y + GetComponent<RectTransform>().sizeDelta.y);
        }
    }

    public void SetIsOn(bool isOn)
    {
        toggle.isOn = isOn;
    }
}
