	#include "UnityCG.cginc"

	uniform float4 _HeightParams;
	uniform float4 _DistanceParams;
	uniform int4 _SceneFogMode;
	uniform float4 _SceneFogParams;
	uniform half _heightFogIntensity;
	uniform sampler2D _EnviroVolumeLightingTex;
	
	// x: scale, y: intensity, z: intensity offset
	uniform float4 _FogNoiseData;
	// x: x velocity, y: z velocity
	uniform float4 _FogNoiseVelocity;
	uniform float4 _EnviroParams; //gametime,distance,height,_hdr
	uniform float _EnviroVolumeDensity;
	uniform float3 _Br;
	uniform float3 _Bm;
	uniform float3 _BmScene;
	uniform float3 _mieG;
	uniform float3 _mieGScene;
	uniform float _Exposure;
	uniform float _SkyLuminance;
	uniform float _scatteringPower;

	uniform float _SunIntensity;
	uniform float _SunDiskSize;
	uniform float _SunDiskIntensity;

	uniform float4 _scatteringColor;
	uniform float4 _sunDiskColor;
	uniform float _SkyColorPower;
	uniform float3 _SunDir;
	uniform float _scatteringStrenght;

	uniform half _distanceFogIntensity;
	uniform half _SkyFogHeight;
	uniform half _skyFogIntensity;
	uniform float _maximumFogDensity;
	uniform float4 _weatherSkyMod;
	uniform float4 _weatherFogMod;
	uniform float _lightning;


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
 half ComputeFogFactor (float coord)
	{
		float fogFac = 0.0;

		if (_SceneFogMode.x == 1) // linear
		{
			fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
		}
		if (_SceneFogMode.x == 2) // exp
		{
			fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
		}
		if (_SceneFogMode.x == 3) // exp2
		{
			fogFac = _SceneFogParams.x * coord; fogFac = exp2(-fogFac*fogFac);
		}

		return saturate(fogFac);
	}

	// Distance fog
	float ComputeDistance (float3 camDir, float zdepth)
	{
		float dist; 
			dist = length(camDir);
		// Built-in fog starts at near plane, so match that by
		// subtracting the near value. Not a perfect approximation
		// if near plane is very large, but good enough.
		dist -= _ProjectionParams.y;
		return dist;
	}

	float ComputeDistance(float depth)
	{
		float dist = depth * _ProjectionParams.z;
		dist -= _ProjectionParams.y;
		return dist;
	}

	// Linear height fog,
	float ComputeHalfSpace (float3 wsDir)
	{
		float3 wpos = _WorldSpaceCameraPos + wsDir;
		float FH = _HeightParams.x;
		float3 C = _WorldSpaceCameraPos;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = (_HeightParams.w * _heightFogIntensity) * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y-FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1-2*k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV+1.0e-5f));
		return g;
	}


	float4 ComputeScattering (float3 viewDir, float2 sunPos)
	{
		   float cosTheta = dot(viewDir, _SunDir);
		   viewDir = viewDir + float3(0.0, 0.1 ,0.0);

		   float zen = acos(saturate(viewDir.y));
		   float alb = (cos(zen) + 0.5 * pow(93.885 - ((zen * 180.0) / 3.141592), - 0.253)); // pi
		   float3 fex  = exp(-(_Br * (4 / alb)  + _Bm * (1.25 / alb)));
		   float rayPhase = 2.5 + pow(cosTheta,1);
		   float  miePhase = _mieG.x / pow(_mieG.y - _mieG.z * cosTheta, 1); 	    
		   float3 BrTheta  = 0.059683 * _Br * rayPhase; 
		   float3 BmTheta  = 0.079577  * _Bm * miePhase;
		   float3 BrmTheta = (BrTheta + BmTheta * 2.0) / ((_Bm + _Br) * 0.75);   
		   float3 scattering = BrmTheta * _SunIntensity * (1.0 - fex);
		   float3 skyFinalize = saturate((pow( 1.0 - fex, 2.0) * 0.234) * (1 - sunPos.x)) * _SkyLuminance;
		   skyFinalize = saturate(lerp(float3(0.1,0.1,0.1), skyFinalize, saturate(dot(viewDir.y + 0.3, float3(0,1,0)))) * (1-fex));
		   scattering *= saturate((lerp(float3(_scatteringPower, _scatteringPower, _scatteringPower), pow(2000.0f * BrmTheta * fex, 0.7), sunPos.y) * 0.05));
		   scattering *= (_SkyLuminance * _scatteringColor.rgb * _scatteringStrenght) * pow((1.0 - fex), 1.0) * sunPos.x;

		   //skyFinalize = lerp(skyFinalize,lerp(skyFinalize,_weatherFogMod, clamp(cosTheta,0,1)), _weatherFogMod.a);
		   float4 fogScattering = float4((scattering + skyFinalize), 1);

			// ToneMapping
			if(_EnviroParams.w == 0)
		  		 fogScattering = saturate( 1.0 - exp(-_Exposure * fogScattering));

			fogScattering = pow(fogScattering,_SkyColorPower);
		 	fogScattering = lerp(fogScattering,lerp(fogScattering,_weatherSkyMod, clamp(cosTheta,0,1)), _weatherSkyMod.a);
			fogScattering = lerp(fogScattering, lerp(fogScattering, _weatherFogMod, _weatherFogMod.a), _weatherFogMod.a);

		 	if(_lightning > 1)
				fogScattering = fogScattering + (_lightning * 0.06); 

			return fogScattering;
	}


	float4 ComputeScatteringClouds(float3 viewDir, float2 sunPos, float time)
	{
		float cosTheta = dot(viewDir, _SunDir);
		viewDir = viewDir + float3(0.0, 0.1, 0.0);

		float zen = acos(saturate(viewDir.y));
		float alb = (cos(zen) + 0.5 * pow(93.885 - ((zen * 180.0) / 3.141592), -0.253)); // pi
		float3 fex = exp(-(_Br * (4 / alb) + _Bm * (1.25 / alb)));
		float rayPhase = 2.5 + pow(cosTheta, 1);
		float  miePhase = _mieG.x / pow(_mieG.y - _mieG.z * cosTheta, 1);
		float3 BrTheta = 0.059683 * _Br * rayPhase;
		float3 BmTheta = 0.079577  * _Bm * miePhase;
		float3 BrmTheta = (BrTheta + BmTheta * 2.0) / ((_Bm + _Br) * 0.75);
		float3 scattering = BrmTheta * _SunIntensity * (1.0 - fex);
		float3 skyFinalize = saturate((pow(1.0 - fex, 2.0) * 0.234) * (1 - sunPos.x)) * _SkyLuminance;
		skyFinalize = saturate(lerp(float3(0.1, 0.1, 0.1), skyFinalize, saturate(dot(viewDir.y + 0.3, float3(0, 1, 0)))) * (1 - fex));
		scattering *= saturate((lerp(float3(_scatteringPower, _scatteringPower, _scatteringPower), pow(2000.0f * BrmTheta * fex, 0.7), sunPos.y) * 0.05));
		scattering *= (_SkyLuminance * _scatteringColor.rgb * _scatteringStrenght) * pow((1.0 - fex), 1.0) * sunPos.x;
		//skyFinalize = lerp(skyFinalize, lerp(skyFinalize, _weatherFogMod, clamp(cosTheta, 0, 1)), _weatherFogMod.a);
		float4 fogScattering = float4((scattering + skyFinalize), 1);

		// ToneMapping
		if (_EnviroParams.w == 0)
			fogScattering = saturate(1.0 - exp(-_Exposure * fogScattering));

		fogScattering = pow(fogScattering, _SkyColorPower);
		fogScattering = lerp(fogScattering, lerp(fogScattering, _weatherSkyMod, clamp(cosTheta, 0, 1)), _weatherSkyMod.a);
		fogScattering = lerp(fogScattering, lerp(fogScattering, _weatherFogMod, _weatherFogMod.a), _weatherFogMod.a);

		return fogScattering*time;
	}

	float4 ComputeScatteringScene (float3 viewDir, float2 sunPos)
	{
		   float cosTheta = dot(viewDir, _SunDir);
		   viewDir = viewDir + float3(0.0, 0.1 ,0.0);
		   float zen = acos(saturate(viewDir.y));
		   float alb = (cos(zen) + 0.5 * pow(93.885 - ((zen * 180.0) / 3.141592), - 0.253)); // pi
		   float3 fex  = exp(-(_Br * (4 / alb)  + _BmScene * (1.25 / alb)));
		   float rayPhase = 2.5 + pow(cosTheta,1);
		   float  miePhase = _mieGScene.x / pow(_mieGScene.y - _mieGScene.z * cosTheta, 1);     
		   float3 BrTheta  = 0.059683 * _Br * rayPhase; 
		   float3 BmTheta  = 0.079577  * _BmScene * miePhase;
		   float3 BrmTheta = (BrTheta + BmTheta * 2.0) / ((_BmScene + _Br) * 0.75);   
		   float3 scattering = BrmTheta * _SunIntensity * (1.0 - fex);
		   float3 skyFinalize = saturate((pow( 1.0 - fex, 2.0) * 0.234) * (1 - sunPos.x)) * _SkyLuminance;
		   skyFinalize = saturate(lerp(float3(0.1,0.1,0.1), skyFinalize, saturate(dot(viewDir.y + 0.3, float3(0,1,0)))) * (1-fex));
		   scattering *= saturate((lerp(float3(_scatteringPower, _scatteringPower, _scatteringPower), pow(2000.0f * BrmTheta * fex, 0.7), sunPos.y) * 0.05));
		   scattering *= (_SkyLuminance * _scatteringColor.rgb * _scatteringStrenght) * pow((1.0 - fex), 1.0) * sunPos.x;
		  // skyFinalize = lerp(skyFinalize, lerp(skyFinalize, _weatherFogMod, clamp(cosTheta, 0, 1)), _weatherFogMod.a);
		   float4 fogScattering = float4((scattering + skyFinalize), 1);

			// ToneMapping
			if(_EnviroParams.w == 0)
		  		 fogScattering = saturate( 1.0 - exp(-_Exposure * fogScattering));

			fogScattering = pow(fogScattering,_SkyColorPower);
			fogScattering = lerp(fogScattering, lerp(fogScattering, _weatherSkyMod, clamp(cosTheta, 0, 1)), _weatherSkyMod.a);
			fogScattering = lerp(fogScattering, lerp(fogScattering, _weatherFogMod, _weatherFogMod.a), _weatherFogMod.a);

		 	if(_lightning > 1)
			fogScattering = fogScattering + (_lightning * 0.06); 

			return fogScattering;
	}

	float4 TransparentFog(float4 clr, float3 wPos,float2 uv, half depth)
	{
		float3 wsDir = wPos - _WorldSpaceCameraPos;
		float g = _DistanceParams.x;
			if (_EnviroParams.y > 0)
			{
				g += ComputeDistance (wsDir, depth);
				g *= _distanceFogIntensity ;
			}

		if (_EnviroParams.z > 0)
			{
				//g += ComputeHalfSpaceWithNoise (wsDir);
				g += ComputeHalfSpace(wsDir);
			} 

		float fogFac = ComputeFogFactor (max(0.0,g));
		fogFac = lerp(_maximumFogDensity,1.0f,fogFac);
		float4 fogClr = float4(0, 0, 0, 0);

#ifdef UNITY_PASS_FORWARDADD
		float4 volumeLighting = float4(0, 0, 0, 0);
#else

	#if ENVIRO_SIMPLE_FOG
			fogClr = unity_FogColor;
	#else
			float2 sunDir;
			sunDir.x = saturate(_SunDir.y + 0.25);
			sunDir.y = saturate(clamp(1.0 - _SunDir.y, 0.0, 0.5));
			fogClr = ComputeScattering(normalize(wsDir), sunDir);
	#endif
		
		//#if UNITY_SINGLE_PASS_STEREO
		 //float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
		 //uv = (uv - scaleOffset.zw) / scaleOffset.xy;
		//#endif

		float4 volumeLighting = tex2D(_EnviroVolumeLightingTex, UnityStereoTransformScreenSpaceTex(uv));
		volumeLighting *= _EnviroParams.x;
#endif
		float4 final = lerp (lerp(fogClr, fogClr + volumeLighting, _EnviroVolumeDensity), lerp(clr, clr + volumeLighting, _EnviroVolumeDensity), fogFac);

		return final;
	}