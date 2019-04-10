using UnityEngine;
using System.Collections;

public class EnviroEvents : MonoBehaviour {


	[System.Serializable]
	public class EnviroActionEvent : UnityEngine.Events.UnityEvent
	{

	}
	//[Header("Time Events")]
	public EnviroActionEvent onHourPassedActions = new EnviroActionEvent();
	public EnviroActionEvent onDayPassedActions = new EnviroActionEvent();
	public EnviroActionEvent onYearPassedActions = new EnviroActionEvent();
	public EnviroActionEvent onWeatherChangedActions = new EnviroActionEvent();
	public EnviroActionEvent onSeasonChangedActions = new EnviroActionEvent();
	public EnviroActionEvent onNightActions = new EnviroActionEvent();
	public EnviroActionEvent onDayActions = new EnviroActionEvent();
	public EnviroActionEvent onZoneChangedActions = new EnviroActionEvent();

	void Start ()
	{
		EnviroSkyMgr.instance.OnHourPassed += () => HourPassed ();
        EnviroSkyMgr.instance.OnDayPassed += () => DayPassed ();
        EnviroSkyMgr.instance.OnYearPassed += () => YearPassed ();
        EnviroSkyMgr.instance.OnWeatherChanged += (EnviroWeatherPreset type) =>  WeatherChanged ();
        EnviroSkyMgr.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) => SeasonsChanged ();
        EnviroSkyMgr.instance.OnNightTime += () => NightTime ();
        EnviroSkyMgr.instance.OnDayTime += () => DayTime ();
        EnviroSkyMgr.instance.OnZoneChanged += (EnviroZone zone) =>  ZoneChanged ();
	}
		
	private void HourPassed()
	{
		onHourPassedActions.Invoke();
	}

	private void DayPassed()
	{
		onDayPassedActions.Invoke();
    }
		
	private void YearPassed()
	{
		onYearPassedActions.Invoke();
	}

	private void WeatherChanged()
	{
		onWeatherChangedActions.Invoke();
	}

	private void SeasonsChanged()
	{
		onSeasonChangedActions.Invoke();
	}

	private void NightTime()
	{
		onNightActions.Invoke ();
	}

	private void DayTime()
	{
		onDayActions.Invoke ();
	}

	private void ZoneChanged()
	{
		onZoneChangedActions.Invoke ();
	}

}
