using UnityEngine;

public class SunVisuals : MonoBehaviour
{
    [SerializeField]
    private Color fogColorDay;

    [SerializeField]
    private Color fogColorNight;

    [SerializeField]
    private Color ambientColorDay;

    [SerializeField]
    private Color ambientColorNight;

    [SerializeField]
    private Light sunDirectionalLight;

    [SerializeField]
    private float crossFadeColorRangeStart = 0.1f;
    [SerializeField]
    private float crossFadeColorRangeEnd = 20.0f;
    private float crossValue = 1.0f;

    private bool day = true;

    [Header("Day and night light color switch")]
    [SerializeField]
    private Color dayLightColor = Color.white;
    [SerializeField]
    private Color nightLightColor = Color.blue;

	public bool Day { get => day; }

	private void Awake()
    {
        InitialUpdate();
    }

    [ContextMenu("Update visuals based on current angles")]
    private void InitialUpdate()
	{
		UpdateVisuals(transform.localEulerAngles);
	}

    public void UpdateVisuals(Vector3 newAngles = default)
    {
        transform.localRotation = Quaternion.Euler(newAngles);

        crossValue = Mathf.InverseLerp(-crossFadeColorRangeEnd, crossFadeColorRangeStart, newAngles.x);

        //Reduce sun strength when we go down the horizon
        sunDirectionalLight.intensity = Mathf.Lerp(0.4f, 1.0f, crossValue);

        //Swap color based on intensity (goes down at night)
        sunDirectionalLight.color = Color.Lerp(nightLightColor, dayLightColor, crossValue);

        //If its night (sun under horizon) switch behaviour to moon
        day = Vector3.Dot(transform.forward, Vector3.down) > -0.1;
        sunDirectionalLight.transform.localEulerAngles = new Vector3(0, (day) ? 0 : 180, 0);

        //Change the fog and ambient color based on cross value
        RenderSettings.fogColor = Color.Lerp(fogColorNight, fogColorDay, crossValue);
        RenderSettings.ambientSkyColor = Color.Lerp(ambientColorNight, ambientColorDay, crossValue);
    }
}
