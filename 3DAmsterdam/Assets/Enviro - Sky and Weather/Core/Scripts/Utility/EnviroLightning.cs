using UnityEngine;
using System.Collections;

public class EnviroLightning : MonoBehaviour {
	
	public void Lightning() 
	{ 
		StartCoroutine(LightningBolt()); 
	}

	public void StopLightning() 
	{ 
		StopAllCoroutines ();
		GetComponent<Light> ().enabled = false;
        EnviroSkyMgr.instance.SetLightningFlashTrigger(0f);
    }


	public IEnumerator LightningBolt()
	{
		GetComponent<Light> ().enabled = true;
		//record the starting/off intensity of the light	
		float defaultIntensity = GetComponent<Light>().intensity;
		//number of flashes this bolt will display
		int flashCount = Random.Range(2, 5);
		int thisFlash = 0; 
		
		while(thisFlash < flashCount){
			GetComponent<Light>().intensity = defaultIntensity * Random.Range(1, 1.5f);
            EnviroSkyMgr.instance.SetLightningFlashTrigger(Random.Range (5f, 10f));
			yield return new WaitForSeconds(Random.Range (0.05f,0.1f));
			GetComponent<Light>().intensity = defaultIntensity;
            EnviroSkyMgr.instance.SetLightningFlashTrigger(1f);
			thisFlash++;
		}

		GetComponent<Light> ().enabled = false;
	}

}
