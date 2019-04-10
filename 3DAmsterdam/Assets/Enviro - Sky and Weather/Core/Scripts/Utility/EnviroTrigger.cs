using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviroTrigger : MonoBehaviour {

	public EnviroInterior myZone;
	public string Name;

	//public bool entered = false;

	void Start () 
	{
		
	}
	

	void Update () 
	{
		
	}
	void OnTriggerEnter (Collider col)
	{
		if (EnviroSkyMgr.instance.GetUseWeatherTag()) {
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag()) {
				EnterExit ();
			}
		} else {
			if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject)) {
				EnterExit ();
			}
		}
	}

	void OnTriggerExit (Collider col)
	{

        if (myZone.zoneTriggerType == EnviroInterior.ZoneTriggerType.Zone)
        {
            if (EnviroSkyMgr.instance.GetUseWeatherTag())
            {
                if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag())
                {
                    EnterExit();
                }
            }
            else
            {
                if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject))
                {
                    EnterExit();
                }
            }
        }
	}
		



	void EnterExit ()
	{
        if (EnviroSkyMgr.instance.lastInteriorZone != myZone)
        {
            if (EnviroSkyMgr.instance.lastInteriorZone != null)
                EnviroSkyMgr.instance.lastInteriorZone.StopAllFading();

            myZone.Enter();
        }
        else
        {
            if (!EnviroSkyMgr.instance.IsInterior())
                myZone.Enter();
            else
                myZone.Exit();
        }
	}

	void OnDrawGizmos () 
	{
		Gizmos.matrix = transform.worldToLocalMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawCube (Vector3.zero,Vector3.one);
	}
}
