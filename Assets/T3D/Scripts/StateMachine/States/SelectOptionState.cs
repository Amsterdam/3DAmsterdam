using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectOptionState : State
{
    [SerializeField]
    private Toggle noModelToggle, uploadedModelToggle, snapToggle;
    [SerializeField]
    private GameObject modelSettingsPanel;

    private void Update()
    {
        modelSettingsPanel.SetActive(!noModelToggle.isOn);
    }

    public override int GetDesiredStateIndex()
    {
        ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel = !noModelToggle.isOn;
        ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall = snapToggle.isOn;
        ServiceLocator.GetService<T3DInit>().HTMLData.HasFile= uploadedModelToggle.isOn;

        print(ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel);
        if (noModelToggle.isOn)
        {
            return 0;
        }
        else if (snapToggle.isOn)
            return 1;
        else
            return 2;
    }
}
