using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    private Action<string> clickAction;

    [SerializeField]
    private Text buttonText;

    public void Select()
    {
        if (clickAction != null) clickAction.Invoke("");
    }

	public void SetAction(string title, Action<string> action)
	{
        buttonText.text = title;
        clickAction = action;
	}
}
