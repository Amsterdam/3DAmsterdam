using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class EnviroVegetationStage {


	public enum GrowState
	{
		Grow,
		Stay,
	}

	[Range(0,100)]
	public float minAgePercent;
	public GrowState growAction;

	public GameObject GrowGameobjectSpring;
	public GameObject GrowGameobjectSummer;
	public GameObject GrowGameobjectAutumn;
	public GameObject GrowGameobjectWinter;

	public bool billboard;
}


[System.Serializable]
public class EnviroVegetationAge {

	public float maxAgeHours = 24f;
	public float maxAgeDays= 60f;
	public float maxAgeYears= 0f;

	public bool randomStartAge;
	public float startAgeinHours = 0f;
	public double birthdayInHours;
	
	public bool Loop = true;
	public int LoopFromGrowStage = 0;
}
	
[System.Serializable]
public class EnviroVegetationSeasons {
	public enum SeasonAction
	{
	SpawnDeadPrefab,
	Deactivate,
	Destroy
	}
	public SeasonAction seasonAction;
	public bool GrowInSpring = true;
	public bool GrowInSummer = true;
	public bool GrowInAutumn = true;
	public bool GrowInWinter = true;
	
}
[AddComponentMenu("Enviro/Vegetation Growth Object")]
public class EnviroVegetationInstance : MonoBehaviour {
	
	[HideInInspector]
	public int id;
	
	public EnviroVegetationAge Age;
	public EnviroVegetationSeasons Seasons;
	public List<EnviroVegetationStage> GrowStages = new List<EnviroVegetationStage>();
	public Vector3 minScale = new Vector3(0.1f,0.1f,0.1f);
	public Vector3 maxScale= new Vector3(1f,1f,1f);
	public float GrowSpeedMod = 1f;
	public GameObject DeadPrefab;
	
	public Color GizmoColor = new Color(255,0,0,255);
	public float GizmoSize = 0.5f;

	/// privates
	private EnviroSeasons.Seasons currentSeason;
	private double ageInHours;
	private double maxAgeInHours;
	private int currentStage;
	private GameObject currentVegetationObject;
	private bool stay = false;
	private bool reBirth = false;
	private bool rescale = true;
	private bool canGrow = true;
	private bool shrink = false;

	void Start () 
	{
		EnviroSkyMgr.instance.RegisterVegetationInstance (this);
		currentSeason = EnviroSkyMgr.instance.GetCurrentSeason();
		maxAgeInHours = EnviroSkyMgr.instance.GetInHours(Age.maxAgeHours, Age.maxAgeDays, Age.maxAgeYears);

        EnviroSkyMgr.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
		{
			SetSeason ();
		};


		if (Age.randomStartAge) 
		{
			Age.startAgeinHours = UnityEngine.Random.Range (0f, (float)maxAgeInHours);
			Age.randomStartAge = false;
		}

		Birth (0, Age.startAgeinHours);
	}

	// Check for correct Setup
	void OnEnable ()
	{
		if (GrowStages.Count < 1)
		{
			Debug.LogError("Please setup GrowStages!");
			this.enabled = false;
		}

		for (int i = 0; i < GrowStages.Count; i++)
		{
			if( GrowStages[i].GrowGameobjectAutumn == null || GrowStages[i].GrowGameobjectSpring == null || GrowStages[i].GrowGameobjectSummer == null || GrowStages[i].GrowGameobjectWinter == null)
			{
				Debug.LogError("One ore more GrowStages missing GrowPrefabs!");
				this.enabled = false;
			}
		}

	}

	void SetSeason ()
	{
		currentSeason = EnviroSkyMgr.instance.GetCurrentSeason();
        VegetationChange ();
	}

	public void KeepVariablesClear ()
	{
		GrowStages [0].minAgePercent = 0f; //First Growstage have to at zero. ever the first object!
		for (int i = 0; i < GrowStages.Count; i++) 
		{
			if (GrowStages[i].minAgePercent > 100f)
				GrowStages[i].minAgePercent = 100f;
		
		}

	}

	public void UpdateInstance ()
	{
		//if (currentSeason != cSeason) //Check for season Change;
		//{
		//	currentSeason = cSeason;
		//	VegetationChange ();
		//}

		if (reBirth)
			Birth (0, 0f);

		if (shrink)
			ShrinkAndDeactivate ();

		if (canGrow) //Only Update ifallowedin this season;
			UpdateGrowth ();

	}

	public void UpdateGrowth ()
	{
		ageInHours = EnviroSkyMgr.instance.GetCurrentTimeInHours() - Age.birthdayInHours;
		KeepVariablesClear ();
		if (!stay) {
			if (currentStage + 1 < GrowStages.Count) 
			{
				if ((maxAgeInHours * (GrowStages [currentStage + 1].minAgePercent / 100) <= ageInHours) && ageInHours > 0)
				{ //Next Growth Stage 	
					currentStage++;
					VegetationChange ();
				} 
				else { //Grow up!
					if (GrowStages[currentStage].growAction == EnviroVegetationStage.GrowState.Grow)
						CalculateScale ();
				}
	
			} else if (!stay) {
				if (ageInHours > maxAgeInHours) {
					if (Age.Loop) {
						currentVegetationObject.SetActive (false);
						if (DeadPrefab != null)
						{
							DeadPrefabLoop ();
						}
						else
						Birth (Age.LoopFromGrowStage, 0f);
					} else {
						stay = true;
					}
				} 
				else  //we aren't dead! so grow up!
				{
					if (GrowStages[currentStage].growAction == EnviroVegetationStage.GrowState.Grow)
						CalculateScale ();
				}
			}
		}
	}

	void DeadPrefabLoop ()
	{
		stay = true;
		GameObject deadObj = (GameObject)Instantiate(DeadPrefab,transform.position,transform.rotation);
		deadObj.transform.localScale = currentVegetationObject.transform.localScale;
		Birth (Age.LoopFromGrowStage, 0f);
		stay = false;
	}

	// Deactivate Colliders for few seconds to non affect dead tree.
	IEnumerator BirthColliders ()
	{
		Collider[] colliders = currentVegetationObject.GetComponentsInChildren<Collider>();
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = false;
		}
	yield return new WaitForSeconds (10f);
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = true;
		}

	}

	void CalculateScale ()	
	{
		if (rescale)
		{
			currentVegetationObject.transform.localScale = minScale;
			rescale = false;
		}
			double agemod = (ageInHours / maxAgeInHours) * GrowSpeedMod;

		currentVegetationObject.transform.localScale = minScale + new Vector3((float)agemod,(float)agemod,(float)agemod);

		if (currentVegetationObject.transform.localScale.y > maxScale.y) 
		{
			currentVegetationObject.transform.localScale = maxScale;
		}

		if (currentVegetationObject.transform.localScale.y < minScale.y)
			currentVegetationObject.transform.localScale = minScale;

			
	}
	
	public void Birth (int stage, float startAge)
	{
		Age.birthdayInHours = EnviroSkyMgr.instance.GetCurrentTimeInHours() - startAge;
		startAge = 0f;
		ageInHours = 0f;
		currentStage = stage;
		rescale = true;
		reBirth = false;
		VegetationChange ();
		StartCoroutine (BirthColliders ());
	}

	void SeasonAction ()
	{
		if (Seasons.seasonAction == EnviroVegetationSeasons.SeasonAction.SpawnDeadPrefab) 
		{
			if(DeadPrefab != null){
			GameObject deadObj = (GameObject) Instantiate(DeadPrefab,transform.position,transform.rotation);
			deadObj.transform.localScale = currentVegetationObject.transform.localScale;}
			currentVegetationObject.SetActive(false);
		}
		else if (Seasons.seasonAction == EnviroVegetationSeasons.SeasonAction.Deactivate) 
		{
			shrink = true;
		}
		else if (Seasons.seasonAction == EnviroVegetationSeasons.SeasonAction.Destroy) 
		{
			Destroy(gameObject);
		}
	}

	void CheckSeason (bool update)
	{

		if (!update && canGrow) {
			SeasonAction ();
			canGrow = false;
		} 
		else if (update && !canGrow)
		{
			canGrow = true;
			reBirth = true;
		}
		else if (!update && !canGrow)
			SeasonAction ();
	}

	void ShrinkAndDeactivate ()
	{
		if (currentVegetationObject.transform.localScale.y > minScale.y)
			currentVegetationObject.transform.localScale = new Vector3(currentVegetationObject.transform.localScale.x - 0.1f * Time.deltaTime,currentVegetationObject.transform.localScale.y - 0.1f * Time.deltaTime,currentVegetationObject.transform.localScale.z - 0.1f * Time.deltaTime);
		else 
		{
			shrink = false;
			currentVegetationObject.SetActive(false);
		}
	}


	public void VegetationChange ()
	{
		canGrow = true;

		if (currentVegetationObject != null)
		currentVegetationObject.SetActive(false);

		switch (currentSeason) {

		case EnviroSeasons.Seasons.Spring:
			currentVegetationObject = GrowStages [currentStage].GrowGameobjectSpring;
			CalculateScale ();
			currentVegetationObject.SetActive (true);
			if(!Seasons.GrowInSpring)
				CheckSeason(false);
			else if (Seasons.GrowInSpring)
				CheckSeason(true);
			break;
		
		case EnviroSeasons.Seasons.Summer:
			currentVegetationObject = GrowStages [currentStage].GrowGameobjectSummer;
			CalculateScale ();
			currentVegetationObject.SetActive (true);
			if(!Seasons.GrowInSummer)
				CheckSeason(false);
			else if (Seasons.GrowInSummer)
				CheckSeason(true);
			break;	
		
		case EnviroSeasons.Seasons.Autumn:
			currentVegetationObject = GrowStages [currentStage].GrowGameobjectAutumn;
			CalculateScale ();
			currentVegetationObject.SetActive (true);
			if(!Seasons.GrowInAutumn)
				CheckSeason(false);
			else if (Seasons.GrowInAutumn)
				CheckSeason(true);
			break;
		
		case EnviroSeasons.Seasons.Winter:
			currentVegetationObject = GrowStages [currentStage].GrowGameobjectWinter;
			CalculateScale ();
			currentVegetationObject.SetActive (true);
			if(!Seasons.GrowInWinter)
				CheckSeason(false);
			else if (Seasons.GrowInWinter)
				CheckSeason(true);
			break;
		}

}
	
	void LateUpdate ()
	{
		// Support for Billboards
		if(GrowStages[currentStage].billboard && canGrow)
			transform.rotation = Camera.main.transform.rotation;
	}

	void OnDrawGizmos () 
	{
		Gizmos.color = GizmoColor;
		Gizmos.DrawCube (transform.position, new Vector3(GizmoSize,GizmoSize,GizmoSize));
	}

}
