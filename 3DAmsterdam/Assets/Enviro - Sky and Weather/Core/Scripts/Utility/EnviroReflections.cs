using UnityEngine;
using System.Collections;

public class EnviroReflections : MonoBehaviour {

	public ReflectionProbe probe;
	public float ReflectionUpdateInGameHours = 1f;

	private double lastUpdate;

	// Use this for initialization
	void Start () 
	{

	if (probe == null)
			probe = GetComponent<ReflectionProbe> ();

        
    }

    void  UpdateProbe ()
	{
		probe.RenderProbe ();
		lastUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours();
    
	}

	// Update is called once per frame
	void Update ()
	{
        if (EnviroSkyMgr.instance.IsAvailable() == false)
            return;

        if (EnviroSkyMgr.instance.GetCurrentTimeInHours() > lastUpdate + ReflectionUpdateInGameHours || EnviroSkyMgr.instance.GetCurrentTimeInHours() < lastUpdate - ReflectionUpdateInGameHours)
			UpdateProbe ();
	}
}
