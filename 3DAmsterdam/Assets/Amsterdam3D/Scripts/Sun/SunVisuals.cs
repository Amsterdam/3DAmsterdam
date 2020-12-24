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

    private Light sunDirectionalLight;

    [SerializeField]
    private float crossFadeColorRange = 20.0f;

    private void Awake()
    {
        sunDirectionalLight = GetComponent<Light>();

        UpdateVisuals(transform.localEulerAngles);
    }

    public void UpdateVisuals(Vector3 newAngles)
    {
        transform.localRotation = Quaternion.Euler(newAngles);

        //Reduce sun strength when we go down the horizon
        sunDirectionalLight.intensity = Mathf.InverseLerp(-crossFadeColorRange, 0.005f, newAngles.x);

        //Change the fog and ambient color based on this intensity
        RenderSettings.fogColor = Color.Lerp(fogColorNight, fogColorDay, sunDirectionalLight.intensity);
        RenderSettings.ambientSkyColor = Color.Lerp(ambientColorNight, ambientColorDay, sunDirectionalLight.intensity);
    }
}
