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

    [SerializeField]
    private Material intensityMaterialTrees;

    private void Awake()
    {
        sunDirectionalLight = GetComponent<Light>();

        UpdateVisuals(transform.localEulerAngles);
    }

    public void UpdateVisuals(Vector3 newAngles)
    {
        transform.localRotation = Quaternion.Euler(newAngles);

        //Reduce sun strength when we go down the horizon
        sunDirectionalLight.intensity = Mathf.InverseLerp(-crossFadeColorRange, 0.1f, newAngles.x);

        //Apply sunlight to tree darkness (who use a very simple unlit shader)
        intensityMaterialTrees.SetFloat("_Light", Mathf.Max(sunDirectionalLight.intensity,0.3f));

        //Change the fog and ambient color based on this intensity
        RenderSettings.fogColor = Color.Lerp(fogColorNight, fogColorDay, sunDirectionalLight.intensity);
        RenderSettings.ambientSkyColor = Color.Lerp(ambientColorNight, ambientColorDay, sunDirectionalLight.intensity);
    }
}
