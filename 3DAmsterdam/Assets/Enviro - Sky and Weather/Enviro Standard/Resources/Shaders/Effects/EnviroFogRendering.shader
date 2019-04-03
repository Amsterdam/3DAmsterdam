
Shader "Enviro/EnviroFogRendering" 
{
	Properties
	{ 
		_EnviroVolumeLightingTex("Volume Lighting Tex",  Any) = ""{}
		_Source("Source",  2D) = "black"{}
	}
	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 3.0
	#pragma multi_compile __ ENVIROVOLUMELIGHT
	#pragma exclude_renderers gles

	//  Start: LuxWater
#pragma multi_compile __ LUXWATER_DEFERREDFOG

#if defined(LUXWATER_DEFERREDFOG)
		sampler2D _UnderWaterMask;
	float4 _LuxUnderWaterDeferredFogParams; // x: IsInsideWatervolume?, y: BelowWaterSurface shift, z: EdgeBlend
#endif
	//  End: LuxWater

	#include "UnityCG.cginc" 
	#include "../Core/EnviroVolumeLightCore.cginc"
	#include "../../../../Core/Resources/Shaders/Core/EnviroFogCore.cginc"

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
    uniform float _noiseScale;
	uniform half _noiseIntensity;
	uniform sampler2D _Clouds;
	uniform float _SunBlocking;
	uniform int _UseDithering;
	uniform sampler3D _FogNoiseTexture;
	uniform sampler2D _DitheringTex;

	struct appdata_t 
	{
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f 
	{
		float4 pos : SV_POSITION;
		float3 texcoord : TEXCOORD0;
		float3 sky : TEXCOORD1;
		float4 uv : TEXCOORD2;
	};

	v2f vert(appdata_img v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.pos = v.vertex * float4(2, 2, 1, 1) + float4(-1, -1, 0, 0);
		o.uv.xy = v.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y > 0)
			o.uv.y = 1 - o.uv.y;
#endif 
		o.sky.x = saturate(_SunDir.y + 0.25);
		o.sky.y = saturate(clamp(1.0 - _SunDir.y, 0.0, 0.5));
		return o;
	}

	// Linear height fog function
	float ComputeHalfSpaceWithNoise(float3 wsDir)
	{
		float3 wpos = _WorldSpaceCameraPos + wsDir;
		float FH = _HeightParams.x;
		float3 C = _WorldSpaceCameraPos;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = (_HeightParams.w * _heightFogIntensity) * V;
		float noise = tex3D(_FogNoiseTexture, frac(wpos * _FogNoiseData.x + float3(_Time.y * _FogNoiseVelocity.x, 0, _Time.y * _FogNoiseVelocity.y)));
		noise = saturate(noise - _FogNoiseData.z) * _FogNoiseData.y;
		aV *= noise;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y - FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1 - 2 * k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
		return g;
	}

	/// Main Fragment Shader
	fixed4 frag(v2f i) : SV_Target
	{
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv));
		float dpth = Linear01Depth(rawDepth);

		float4x4 proj, eyeToWorld;
		if (unity_StereoEyeIndex == 0)
		{
			proj = _LeftViewFromScreen;
			eyeToWorld = _LeftWorldFromView;
		}
		else
		{
			proj = _RightViewFromScreen;
			eyeToWorld = _RightWorldFromView;
		}

		//bit of matrix math to take the screen space coord (u,v,depth) and transform to world space
		float2 uvClip = i.uv * 2.0 - 1.0;
		float clipDepth = rawDepth; // Fix for OpenGl Core thanks to Lars Bertram
		clipDepth = (UNITY_NEAR_CLIP_VALUE < 0) ? clipDepth * 2 - 1 : clipDepth;
		float4 clipPos = float4(uvClip, clipDepth, 1.0);
		float4 viewPos = mul(proj, clipPos); // inverse projection by clip position
		viewPos /= viewPos.w; // perspective division
		float4 wsPos = float4(mul(eyeToWorld, viewPos).xyz, 1);
		float4 wsDir = wsPos - float4(_WorldSpaceCameraPos, 0);
		float3 viewDir = normalize(wsDir);

		 
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		half fogFac = 0;  
		float4 finalFog = 0;
		float g = _DistanceParams.x;
		half gHeight = 0; 
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		   
		//Scene 
		if (dpth < 0.99999)  
		{
			// Calculate Distance Fog
			if (_EnviroParams.y > 0)
			{   
				g += ComputeDistance(wsDir, dpth);
				g *= _distanceFogIntensity;
			}  

			if (_EnviroParams.z > 0)
			{
				gHeight = ComputeHalfSpaceWithNoise(wsDir);
				//gAdd = ComputeHalfSpace(wsDir);
			}    

			// Add Height Fog 
			g += gHeight;
			 
			// Compute fog amount
			fogFac = ComputeFogFactor(max(0.0, g));
			fogFac = lerp(_maximumFogDensity, 1.0f, fogFac);

			finalFog = ComputeScatteringScene(viewDir, i.sky.xy);
		}
		else //SKY
		{
			if (_EnviroParams.z > 0)
			{ 
				gHeight = ComputeHalfSpace(wsDir);
			}

			half fogFacSky = ComputeFogFactor(max(0.0, gHeight));

			float f = saturate(_SkyFogHeight * viewDir.y); //saturate((_SkyFogHeight * dot(normalize(wsPos - _WorldSpaceCameraPos.xyz), float3(0, 1, 0))));
			f = pow(f, _skyFogIntensity);
			 
			fogFac = (clamp(f, 0, 1));

			if (fogFac > fogFacSky) 
				fogFac = fogFacSky;

			float4 skyFog = ComputeScattering(viewDir, i.sky.xy);

			finalFog = skyFog;
		}

		//Dithering
		if (_UseDithering == 1)
		{
			float4 dither = tex2D(_DitheringTex, i.uv / 8.0).r / 64.0 - (1.0 / 128.0);
			finalFog = float4(finalFog.rgb + dither.rgb, finalFog.a);
		}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Composing

//  Start: LuxWater
#if defined(LUXWATER_DEFERREDFOG)
		half4 fogMask = tex2D(_UnderWaterMask, UnityStereoTransformScreenSpaceTex(i.uv));
		float watersurfacefrombelow = DecodeFloatRG(fogMask.ba);

		//	Get distance and lower it a bit in order to handle edge blending artifacts (edge blended parts would not get ANY fog)
		float dist = (watersurfacefrombelow - dpth) + _LuxUnderWaterDeferredFogParams.y * _ProjectionParams.w;
		//	Fade fog from above water to below water
		float fogFactor = saturate(1.0 + _ProjectionParams.z * _LuxUnderWaterDeferredFogParams.z * dist);
		//	Clamp above result to where water is actually rendered
		fogFactor = (fogMask.r == 1) ? fogFactor : 1.0;
		//  Mask fog on underwarter parts - only if we are inside a volume (bool... :( )
		if (_LuxUnderWaterDeferredFogParams.x) {
			fogFactor *= saturate(1.0 - fogMask.g * 8.0);
			if (dist < -_ProjectionParams.w * 4 && fogMask.r == 0 && fogMask.g < 1.0) {
				fogFactor = 1.0;
			}
		}
		//	Tweak fog factor
		fogFac = lerp(1.0, fogFac, fogFactor);
#endif
//  End: LuxWater 



		float4 final;
		float4 source = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));
		
		#if defined (ENVIROVOLUMELIGHT)
			float4 volumeLighting = tex2D(_EnviroVolumeLightingTex, UnityStereoTransformScreenSpaceTex(i.uv));
			volumeLighting *= _EnviroParams.x;

			//  Start: LuxWater
#if defined(LUXWATER_DEFERREDFOG)
			volumeLighting *= fogFactor;
#endif
			//  End: LuxWater 

			final = lerp (lerp(finalFog, finalFog + volumeLighting, _EnviroVolumeDensity), lerp(source, source + volumeLighting, _EnviroVolumeDensity), fogFac);
		#else
			final = lerp (finalFog, source, fogFac);
		#endif

		return final;
		}
		ENDCG
		}
	}

	Fallback Off
}
