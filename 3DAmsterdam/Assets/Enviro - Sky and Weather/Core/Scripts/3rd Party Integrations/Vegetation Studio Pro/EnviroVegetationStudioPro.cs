// Vegetation Studio Pro Integration v0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationStudio;
#endif
[AddComponentMenu("Enviro/Integration/VS Pro Integration")]
public class EnviroVegetationStudioPro : MonoBehaviour
{

#if VEGETATION_STUDIO_PRO

    public bool setWindZone = true;
    public bool syncRain = true;
    public bool syncSnow = true;

    void Start()
    {
        if (VegetationStudioManager.Instance == null || EnviroSkyMgr.instance == null)
            return;
   
        if (setWindZone)
        {
            for (int i = 0; i < VegetationStudioManager.Instance.VegetationSystemList.Count; i++)
            {
                VegetationStudioManager.Instance.VegetationSystemList[i].SelectedWindZone = EnviroSkyMgr.instance.Components.windZone;
            }
        }
    }

    void Update()
    {
        if(VegetationStudioManager.Instance == null || EnviroSkyMgr.instance == null)
            return;

        //Update Vegetation Systems
        for(int i = 0; i < VegetationStudioManager.Instance.VegetationSystemList.Count; i++)
        {

         if ((syncRain || syncSnow) && (VegetationStudioManager.Instance.VegetationSystemList[i].EnvironmentSettings.RainAmount != EnviroSkyMgr.instance.Weather.wetness) || (VegetationStudioManager.Instance.VegetationSystemList[i].EnvironmentSettings.SnowAmount != EnviroSkyMgr.instance.Weather.snowStrength))
                VegetationStudioManager.Instance.VegetationSystemList[i].RefreshMaterials();

            if(syncRain)
              VegetationStudioManager.Instance.VegetationSystemList[i].EnvironmentSettings.RainAmount = EnviroSkyMgr.instance.Weather.wetness;
            if(syncSnow)
              VegetationStudioManager.Instance.VegetationSystemList[i].EnvironmentSettings.SnowAmount = EnviroSkyMgr.instance.Weather.snowStrength;           
        }
    }
#endif
}
