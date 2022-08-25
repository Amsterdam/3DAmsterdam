using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class DisableMainBuildingToggle : MonoBehaviour
{
    private Toggle disableMainBuildingToggle;
    private int activeLod;
    //private CityObjectType uitbouwType;

    private void Awake()
    {
        disableMainBuildingToggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        disableMainBuildingToggle.onValueChanged.AddListener(ToggleMainBuilding);
    }

    private void OnDisable()
    {
        disableMainBuildingToggle.onValueChanged.RemoveListener(ToggleMainBuilding);
    }

    private void ToggleMainBuilding(bool active)
    {
        CityJSONToCityObject building = RestrictionChecker.ActiveBuilding.GetComponent<CityJSONToCityObject>();
        var uitbouw = RestrictionChecker.ActiveUitbouw as UploadedUitbouw;//.GetComponent<CityObject>();
        if (active)
        {
            building.SetMeshActive(activeLod);
            CityJSONFormatter.AddCityObejct(building);
            //uitbouw.Type = uitbouwType;
            uitbouw.ReparentToMainBuilding(building);
        }
        else
        {
            //save data to set back when toggle is turned on again
            activeLod = building.ActiveLod;
            //uitbouwType = uitbouw.Type;

            building.SetMeshActive(-1);
            CityJSONFormatter.RemoveCityObject(building);
            //uitbouw.Type = CityObjectType.Building;
            uitbouw.UnparentFromMainBuilding();
        }
    }
}
