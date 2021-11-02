using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

public class BoundaryFeatureButton : WorldPointFollower
{
    public Button Button { get; private set; }

    public override void Awake()
    {
        base.Awake();
        Button = GetComponent<Button>();
    }
    public void SetClickFunction(UnityEngine.Events.UnityAction call)
    {
        Button.onClick.AddListener(call);
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveAllListeners();
    }
}
