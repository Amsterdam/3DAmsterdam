using UnityEngine;
using System.Collections;
#if ENVIRO_MEGASPLAT_SUPPORT
[AddComponentMenu("Enviro/Integration/MegaSplat Integration")]
#endif
public class EnviroMegaSplatIntegration : MonoBehaviour {
#if ENVIRO_MEGASPLAT_SUPPORT
    public MegaSplatTerrainManager MegaSplatTerrianMgr;
	private Material MegaSplatMaterial;

	[Header("Synchronize Weather")]
	public bool UpdateWetness = true;
	public bool UpdateSnow = true;
	public bool UpdatePuddles = true;
	public bool UpdateRainRipples = true;

    [Header("Puddle Settings")]
    [Range(0f,1f)]
    public float puddleStartWetness = 0.25f;

    private Vector4 MegaSplatWetness;
	private float puddleBlend;

	void Start () 
	{

        if (MegaSplatTerrianMgr == null)
            MegaSplatTerrianMgr = GameObject.FindObjectOfType<MegaSplatTerrainManager>();

        if (MegaSplatTerrianMgr != null)
            MegaSplatMaterial = MegaSplatTerrianMgr.templateMaterial;

        if (MegaSplatMaterial != null) 
			MegaSplatWetness = MegaSplatMaterial.GetVector ("_GlobalPorosityWetness");
	}
  
	void Update () 
	{
		if (MegaSplatTerrianMgr == null || MegaSplatMaterial == null || EnviroSkyMgr.instance == null)
			return;

		if (UpdateSnow){
			MegaSplatMaterial.SetFloat ("_SnowAmount", EnviroSkyMgr.instance.GetSnowIntensity());
		}

		if (UpdateWetness) {
			MegaSplatMaterial.SetVector ("_GlobalPorosityWetness", new Vector4(MegaSplatWetness.x,EnviroSkyMgr.instance.GetWetnessIntensity(),MegaSplatWetness.z,MegaSplatWetness.w));
		}
			
		if (UpdatePuddles) {
			puddleBlend = Mathf.Clamp(EnviroSkyMgr.instance.GetWetnessIntensity() - puddleStartWetness, 0f,1f) * 60f;
			puddleBlend = Mathf.Clamp (puddleBlend, 1f, 60f);
			MegaSplatMaterial.SetFloat ("_PuddleBlend",puddleBlend);
		}

		if (UpdateRainRipples) {
            MegaSplatMaterial.SetFloat("_RainIntensity", EnviroSkyMgr.instance.GetWetnessIntensity());
        }

        MegaSplatTerrianMgr.Sync();

    }
#endif
}
