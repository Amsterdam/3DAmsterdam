/////////////////////////////////////////////////////////////////////////////////////////////////////////
//////  EnviroTerrainSeasons - Switches Terrain Textures and grass color according current seasons //////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
/// 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnviroTerrainSeasonsChangeOrder
{
	public int terrainTextureID = 0;

	[Header("Textures")]
	public Texture2D SpringTexture;
	public Texture2D SpringNormal;
	public Texture2D SummerTexture;
	public Texture2D SummerNormal;
	public Texture2D AutumnTexture;
	public Texture2D AutumnNormal;
	public Texture2D WinterTexture;
	public Texture2D WinterNormal;
	public Vector2 tiling = new Vector2(10f,10f);
}


//[AddComponentMenu("Enviro/Utility/Seasons for Terrains")]
//public class EnviroTerrainSeasons : MonoBehaviour {
	
//	public Terrain terrain;

//	[Header("Terrain Textures")]
//	public bool ChangeTextures = true;
//	public List <EnviroTerrainSeasonsChangeOrder> TextureChanges = new List<EnviroTerrainSeasonsChangeOrder> ();


//	[Header("Grass Tint")]
//	public bool ChangeGrassTint = true;
//	public Color SpringGrassColor = Color.white;
//	public Color SummerGrassColor = Color.white;
//	public Color AutumnGrassColor = Color.white;
//	public Color WinterGrassColor = Color.white;

//	[Header("Grass Wind")]
//	public bool ChangeGrassWind = true;
//	public float windSpeedModificator = 5f;
//	public float windSizeModificator = 5f;

//	SplatPrototype[] textureInSplats  = new SplatPrototype[1];
//	SplatPrototype[] texturesIn;   

//	void Start () 
//	{
//		if (terrain == null)
//			terrain = GetComponent<Terrain> ();
//		texturesIn = terrain.terrainData.splatPrototypes;
//		UpdateSeason ();

//        EnviroSkyMgr.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
//		{
//			UpdateSeason ();
//		};

//	}
		
//	// Check for correct Setup
//	void OnEnable ()
//	{
//		if (ChangeTextures)
//		{
//			for (int i = 0; i < TextureChanges.Count; i++) {

//				if (TextureChanges[i].SpringTexture == null) {
//					Debug.LogError ("Please assign a spring texture in Inspector!");
//					this.enabled = false;
//				}
//				if (TextureChanges[i].SummerTexture == null) {
//					Debug.LogError ("Please assign a summer texture in Inspector!");
//					this.enabled = false;
//				}
//				if (TextureChanges[i].AutumnTexture == null) {
//					Debug.LogError ("Please assign a autumn texture in Inspector!");
//					this.enabled = false;
//				}
//				if (TextureChanges[i].WinterTexture == null) {
//					Debug.LogError ("Please assign a winter texture in Inspector!");
//					this.enabled = false;
//				}
//				if (TextureChanges[i].terrainTextureID < 0) {
//					Debug.LogError ("Please configure Texture ChangeSlot IDs!");
//					this.enabled = false;
//				}
//			}
//		}
//	}

//	void ChangeGrassColor (Color ChangeToColor)
//	{
//		terrain.terrainData.wavingGrassTint = ChangeToColor;
//	}

	
//	void ChangeTexture(Texture2D inTexture, int id, Vector2 tiling)
//	{        
//		textureInSplats = texturesIn;
//		textureInSplats[id].texture = inTexture; // texture here
//		textureInSplats[id].tileSize= tiling; //tiling size
//		terrain.terrainData.splatPrototypes = textureInSplats;

//	}

//	void ChangeTexture(Texture2D inTexture,Texture2D inNormal, int id, Vector2 tiling)
//	{        
//		textureInSplats = texturesIn;
//		textureInSplats[id].texture = inTexture; // texture here
//		textureInSplats[id].normalMap = inNormal; // texture here
//		textureInSplats[id].tileSize= tiling; //tiling size
//		terrain.terrainData.splatPrototypes = textureInSplats;
//	}


//	void UpdateSeason ()
//	{
//		switch (EnviroSkyMgr.instance.GetCurrentSeason())
//		{
//		case EnviroSeasons.Seasons.Spring:
//			for (int i = 0 ; i < TextureChanges.Count;i++)
//			{
//				if (ChangeTextures)
//				{
//					if (TextureChanges[i].SpringNormal != null)
//						ChangeTexture(TextureChanges[i].SpringTexture,TextureChanges[i].SpringNormal,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);
//					else
//						ChangeTexture(TextureChanges[i].SpringTexture,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);

//					terrain.Flush();
//				}
//			}
//			if(ChangeGrassTint)
//				ChangeGrassColor(SpringGrassColor);
//			break;
			
//		case EnviroSeasons.Seasons.Summer:
//			for (int i = 0 ; i < TextureChanges.Count;i++)
//			{
//				if (ChangeTextures)
//				{
//					if (TextureChanges[i].SummerNormal != null)
//						ChangeTexture(TextureChanges[i].SummerTexture,TextureChanges[i].SummerNormal,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);
//					else
//						ChangeTexture(TextureChanges[i].SummerTexture,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);

//					terrain.Flush();
//				}
//			}
//			if(ChangeGrassTint)
//				ChangeGrassColor(SummerGrassColor);
//			break;
			
//		case EnviroSeasons.Seasons.Autumn:
//			for (int i = 0 ; i < TextureChanges.Count;i++)
//			{
//				if (ChangeTextures)
//				{
//					if (TextureChanges[i].AutumnNormal != null)
//						ChangeTexture(TextureChanges[i].AutumnTexture,TextureChanges[i].AutumnNormal,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);
//					else
//						ChangeTexture(TextureChanges[i].AutumnTexture,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);

//					terrain.Flush();
//				}
//			}
//			if(ChangeGrassTint)
//				ChangeGrassColor(AutumnGrassColor);
//			break;
			
//		case EnviroSeasons.Seasons.Winter:
//			for (int i = 0 ; i < TextureChanges.Count;i++)
//			{
//				if (ChangeTextures)
//				{
//					if (TextureChanges[i].WinterNormal != null)
//						ChangeTexture(TextureChanges[i].WinterTexture,TextureChanges[i].WinterNormal,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);
//					else
//						ChangeTexture(TextureChanges[i].WinterTexture,TextureChanges[i].terrainTextureID,TextureChanges[i].tiling);

//					terrain.Flush();
//				}
//			}
//			if(ChangeGrassTint)
//				ChangeGrassColor(WinterGrassColor);
//			break;
//		}
//	}

//	void Update ()
//	{
//        if (EnviroSkyMgr.instance == null)
//            return;

//        if (ChangeGrassWind && EnviroSkyMgr.instance.GetCurrentWeatherPreset() != null) 
//		{
//			terrain.terrainData.wavingGrassStrength = EnviroSkyMgr.instance.GetCurrentWeatherPreset().WindStrenght * windSpeedModificator;
//			terrain.terrainData.wavingGrassSpeed = EnviroSkyMgr.instance.GetCurrentWeatherPreset().WindStrenght * windSizeModificator;
//		}
//	}
//}
