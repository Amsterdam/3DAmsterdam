Shader "Enviro/RaymarchClouds"
{
	Properties
	{

	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Tags{ "RenderType" = "Opaque" }

		Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#pragma exclude_renderers gles 
		#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
		#include "UnityCG.cginc"
		#include "../../../Core/Resources/Shaders/Core/EnviroFogCore.cginc"

		struct appdata
		{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 sky : TEXCOORD1;
	};

	uniform float4x4 _InverseProjection;
	uniform float4x4 _InverseRotation;
	uniform float4x4 _InverseProjection_SP;
	uniform float4x4 _InverseRotation_SP;

	v2f vert(appdata_img v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.position = UnityObjectToClipPos(v.vertex); 
		o.uv = v.texcoord;
		o.sky.x = saturate(_SunDir.y + 0.25);
		o.sky.y = saturate(clamp(1.0 - _SunDir.y, 0.0, 0.5));
		return o;
	}

	uniform sampler3D _Noise;
	uniform sampler3D _NoiseLow;
	uniform sampler3D _DetailNoise;
	uniform sampler2D _WeatherMap;
	uniform sampler2D _CurlNoise;
	//uniform sampler2D _bayerNoise;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

	uniform float _CloudBaseColor;
	uniform float _CloudTopColor;
	uniform float4 _CloudsParameter;
	uniform float4 _Steps;
	uniform float _AlphaCoef;
	uniform float _ExtinctionCoef;
	uniform float _HgPhaseFactor;
	uniform float _BaseNoiseUV;
	uniform float _DetailNoiseUV;
	uniform float _CloudDensityScale;
	uniform float4 _LightColor;
	uniform float4 _MoonLightColor;
	uniform float _AmbientLightIntensity;
	uniform float _SunLightIntensity;
	uniform float _CloudsType;
	uniform float _CloudsCoverage;
	uniform float4 _CloudsAnimation;
	uniform float _Tonemapping;
	uniform float _CloudsExposure;
	uniform float3 _LightDir;
	uniform float _gameTime;
	uniform float _PrimAtt;
	uniform float _SecAtt;
	uniform float _GlobalCoverage;
	uniform float _SkyBlending;
	uniform float _stepsInDepth;
	uniform float _LODDistance;

	const float env_inf = 1e10;
	const float3 RandomUnitSphere[6] = { { 0.4f, -0.6f, 0.1f },{ 0.2f, -0.3f, -0.2f },{ -0.9f, 0.3f, -0.1f },{ -0.5f, 0.5f, 0.4f },{ -1.0f, 0.3f, 0.2f },{ 0.3f, -0.9f, 0.4f } };

	float2 raySphereIntersect(float3 ro, float3 rd, float3 so, float radius)
	{
		float3 D = ro - so;
		float b = dot(rd, D);
		float c = dot(D, D) - radius * radius;
		float Delta = b * b - c;
		if (Delta < 0.0) {
			return float2(-env_inf, env_inf);
		}
		Delta = sqrt(Delta);
		return float2(-b - Delta, -b + Delta);
	}

	float2	ComputeBothSphereIntersections(float3 _Pos, float3 _Direction, float3 _SphereCenter, float _SphereRadius)
	{
		float3 D = _Pos - _SphereCenter;
		float b = dot(_Direction, D);
		float c = dot(D, D) - _SphereRadius*_SphereRadius;
		float Delta = b*b - c;
		float SqrtDelta = sqrt(Delta);
		return lerp(float2(-b - SqrtDelta, -b + SqrtDelta), float2(+env_inf, -env_inf), saturate(-10000.0 * Delta));
	}

	// Realtime Volumetric Rendering Course Notes by Patapom (page 15)
	float exponential_integral(float z) {
		return 0.5772156649015328606065 + log(1e-4 + abs(z)) + z * (1.0 + z * (0.25 + z * ((1.0 / 18.0) + z * ((1.0 / 96.0) + z * (1.0 / 600.0))))); // For x!=0
	}

	// Realtime Volumetric Rendering Course Notes by Patapom (page 15)
	float3 CalculateAmbientLighting(float altitude, float extinction_coeff, float4 skyColor) 
	{
		float ambient_term = 0.6 * saturate(1.0 - altitude);
		float3 isotropic_scattering_top = (skyColor.rgb * _CloudTopColor * _LightColor) * max(0.0, exp(ambient_term) - ambient_term * exponential_integral(ambient_term));

		ambient_term = -extinction_coeff * altitude;
		float3 isotropic_scattering_bottom = skyColor.rgb * _CloudBaseColor * max(0.0, exp(ambient_term) - ambient_term * exponential_integral(ambient_term)) * 1.5;

		isotropic_scattering_top *= saturate(altitude);

		return (isotropic_scattering_top)+(isotropic_scattering_bottom);
	}

	float PhaseHenyeyGreenStein(float inScatteringAngle, float g)
	{
		return ((1.0 - g * g) / pow((1.0 + g * g - 2.0 * g * inScatteringAngle), 3.0 / 2.0)) / (4.0 * 3.14159);
	}

	float Beer(float opticalDepth)
	{
		return max(exp(-opticalDepth), (exp(-opticalDepth * 0.005f) * 0.7));
	}

	float GetAlpha(float opticalDepth)
	{
		return exp(-2 * 0.005f * opticalDepth);
	}

	float Remap(float org_val,float org_min,float org_max,float new_min,float new_max)
	{
		return new_min + saturate(((org_val - org_min) / (org_max - org_min))*(new_max - new_min));
	}

	float3 decode_curl(float3 c) {
		return (c - 0.5) * 2.0;
	}

	float3 get_curl_offset(float3 pos, float curl_amplitude, float curl_frequency, float altitude) {
		float4 curl_data = tex2Dlod(_CurlNoise, float4(pos.xy * curl_frequency,0,0));
		return decode_curl(curl_data.rgb) * curl_amplitude * (1.0 - altitude * 0.5);
	}

	float4 GetHeightGradient(float cloudType)
	{
		const float4 CloudGradient1 = float4(0.0, 0.05, 0.1, 0.2);
		const float4 CloudGradient2 = float4(0.0, 0.2, 0.4, 0.8);
		const float4 CloudGradient3 = float4(0.0, 0.1, 0.6, 0.9);

		float a = 1.0 - saturate(cloudType * 2.0);
		float b = 1.0 - abs(cloudType - 0.5) * 2.0;
		float c = saturate(cloudType - 0.5) * 2.0;

		return CloudGradient1 * a + CloudGradient2 * b + CloudGradient3 * c;
	}

	float GradientStep(float a, float4 gradient)
	{
		return smoothstep(gradient.x, gradient.y, a) - smoothstep(gradient.z, gradient.w, a);
	}

	float3 GetWeather(float3 pos)
	{
		float2 uv = pos.xz * 0.00001 + 0.5;
		return tex2Dlod(_WeatherMap, float4(uv, 0.0, 0.0));
	}


	float CalculateCloudDensity(float3 pos, float height, float3 weather, float dist, bool details)
	{
		const float baseFreq = 1e-5;
		
		float4 coord = float4(pos * baseFreq * _BaseNoiseUV, 0.0);
		coord.xyz += float3(_CloudsAnimation.x, -_Time.x * 0.15 ,_CloudsAnimation.y );
		float4 baseNoise = 0;
		
		if(dist > _LODDistance)
			baseNoise = tex3Dlod(_Noise, coord);
		else
			baseNoise = tex3Dlod(_NoiseLow, coord);

		float low_freq_fBm = (baseNoise.g * 0.625) + (baseNoise.b * 0.25) + (baseNoise.a * 0.125);
		float base_cloud = Remap(baseNoise.r, -(1.0 - low_freq_fBm), 1.0, 0.0, 1.0);

		float heightGradient = GradientStep(height, GetHeightGradient((weather.b)*_CloudsType));
		base_cloud *= heightGradient;

		float cloud_coverage = 1 - weather.r;

		cloud_coverage = pow(cloud_coverage, Remap(height, 0.7, 0.8, 1.0, lerp(1.0, 0.5, 0.0)));

		float cloudDensity = Remap(base_cloud, cloud_coverage, 1.0, 0.0, 1.0);
		cloudDensity = Remap(cloudDensity, saturate(height * 0.75 / _CloudsCoverage), 1.0, 0.0, 1.0);
		cloudDensity *= cloudDensity;

		//DETAIL
		[branch]
		if (details && dist > 0.1)
		{
			coord = float4(pos * baseFreq * _DetailNoiseUV, 0.0);
			coord.xyz += float3(_CloudsAnimation.x , -_Time.x * 0.15, _CloudsAnimation.y );
		//	float3 curl = get_curl_offset(pos, 5.0, 0, height);
			float3 detailNoise = tex3Dlod(_DetailNoise, coord ).rgb;
			float high_freq_fBm = (detailNoise.r * 0.625) + (detailNoise.g * 0.25) + (detailNoise.b * 0.125);
			float high_freq_noise_modifier = lerp(high_freq_fBm,1.0f - high_freq_fBm, saturate(height * 10));
			cloudDensity = Remap(cloudDensity, high_freq_noise_modifier * (1.4 * clamp(height,0.1,1) ), 1.0, 0.0, 1.0);
		}
		///
		return saturate(_CloudDensityScale * cloudDensity);
	}

	float calcHeight(float3 pos, float3 planetCenter)
	{
		float d = distance(pos, planetCenter);
		float h = d - _CloudsParameter.w - _CloudsParameter.x;
		return saturate(h / _CloudsParameter.z);
	}

	float GetLightEnergy(float3 p, float height_fraction, float dl, float ds_loded, float phase_probability, float cos_angle, float step_size, float brightness)
	{
		// attenuation – difference from slides – reduce the secondary component when we look toward the sun.
		float primary_attenuation = exp(-dl);
		float secondary_attenuation = exp(-dl * 0.25) * 0.7;
		float attenuation_probability = max(Remap(cos_angle, 0.7, 1.0, _SecAtt, _SecAtt * 0.25), _PrimAtt);

		// in-scattering – one difference from presentation slides – we also reduce this effect once light has attenuated to make it directional.
		float depth_probability = lerp(0.05 + pow(ds_loded, Remap(height_fraction, 0.3, 0.85, 0.5, 2.0)), 1.0, saturate(dl / step_size));
		float vertical_probability = pow(Remap(height_fraction, 0.07, 0.14, 0.1, 1.0), 0.8);
		float in_scatter_probability = depth_probability * vertical_probability;

		float light_energy = attenuation_probability * in_scatter_probability * phase_probability * brightness;

		return light_energy;
	}

	float3 GetDensityAlongRay(float3 pos, float3 PlanetCenter, float3 LightDirection, float3 weather, float dist, float h)
	{
		float sunRayStepLength = (_CloudsParameter.z ) / 5;
		float3 sunRayStep = 0;

		sunRayStep = LightDirection * sunRayStepLength;

		float opticalDepth = 0.0;
		float cloudDens = CalculateCloudDensity(pos, h, weather, dist, true);

		pos += 0.2 * sunRayStep;

		[loop]
		for (int i = 0; i < 5; i++)
		{
			float3 randomOffset = RandomUnitSphere[i] * sunRayStepLength * 1 * ((float)(i + 1));
			float3 samplePos = pos + randomOffset;
			float height = calcHeight(samplePos, PlanetCenter);
			float cloudDensity = CalculateCloudDensity(samplePos, height, weather, dist, true);
			opticalDepth += cloudDensity * sunRayStepLength;
			pos += sunRayStep;
		}

		float extinct = Beer(opticalDepth);
		float powder_term = (1.0 - saturate(exp(-1 * sunRayStepLength * 2.0)) * extinct) * 0.5;
		return float3(extinct, cloudDens, powder_term);
	}


	float4 frag(v2f i) : SV_Target
	{
		float4 cameraRay = float4(i.uv * 2.0 - 1.0, 1.0, 1.0);
		//World Space
		float3 EyePosition = _WorldSpaceCameraPos;
		//Workaround for large scale games where player position will be resetted.
		//float3 EyePosition = float3(0.0,_WorldSpaceCameraPos.y, 0.0);
		float3 ray = 0;

#if UNITY_SINGLE_PASS_STEREO
		if (unity_StereoEyeIndex == 0)
		{
			cameraRay = mul(_InverseProjection, cameraRay);
			cameraRay = cameraRay / cameraRay.w;
			ray = normalize(mul((float3x3)_InverseRotation, cameraRay.xyz));
		}
		else
		{
			cameraRay = mul(_InverseProjection_SP, cameraRay);
			cameraRay = cameraRay / cameraRay.w;
			ray = normalize(mul((float3x3)_InverseRotation_SP, cameraRay.xyz));
		}
#else
		cameraRay = mul(_InverseProjection, cameraRay);
		cameraRay = cameraRay / cameraRay.w;
		ray = normalize(mul((float3x3)_InverseRotation, cameraRay.xyz));
#endif

		float4 color = 0.0;
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv));
		float dpth = Linear01Depth(rawDepth);
		
		float3 wsDir = dpth * ray;
		float3 viewDir = normalize(wsDir);

		float4 sky = ComputeScatteringClouds(viewDir, i.sky.xy, _gameTime);

		float3 LightDirection = _LightDir;
		float3 LightColor = _LightColor.rgb;

		if (_gameTime > 0.55)
			LightColor = _MoonLightColor.rgb;

		float pRad = _CloudsParameter.w;
		float3 pCent = float3(0.0, -pRad, 0.0);

		//float2 hitDistBottom = raySphereIntersect(EyePosition, ray, pCent, _CloudsParameter.w + _CloudsParameter.x);
		//float2 hitDistTop = raySphereIntersect(EyePosition, ray, pCent, _CloudsParameter.w + _CloudsParameter.y);

		float2 hitDistBottom = ComputeBothSphereIntersections(EyePosition, ray, pCent, _CloudsParameter.w + _CloudsParameter.x);
		float2 hitDistTop = ComputeBothSphereIntersections(EyePosition, ray, pCent, _CloudsParameter.w + _CloudsParameter.y);

		float2 hitDistance;

		float h = EyePosition.y + pRad;
		float ch = length(EyePosition - pCent) - _CloudsParameter.w;

		if (ch < _CloudsParameter.x)
		{
			hitDistance = float2(hitDistBottom.y, hitDistTop.y);

			if (ray.y < -0.1f)
				//clip(h * h * (1.0 - ray.y * ray.y) - pRad * pRad);
				return float4(0,0,0,0);
				

		}
		else if (ch > _CloudsParameter.y)
		{
			if (hitDistBottom.x > -env_inf)
				hitDistance = float2(hitDistTop.x, hitDistBottom.x);
			else
				hitDistance = float2(0.0, -1.0);

		}
		else
		{
			if (hitDistBottom.x < 0.0)
				hitDistance = float2(0.0, hitDistTop.y);
			else
				hitDistance = float2(0.0, hitDistBottom.x);
		}

		hitDistance.x = max(0.0, hitDistance.x);
		//clip(hitDistance.y - hitDistance.x);


		float MeanFreePathKm = 120 * (1.0 + hitDistance.x) / (1 * lerp(1.0, 0.025, smoothstep(-0.2, -0.6, 1)));
		hitDistance.y = min(hitDistance.y, hitDistance.x + MeanFreePathKm);

		/////////////////////////


		/////////////////////

		float inScatteringAngle = dot(ray, LightDirection);
		int steps = (int)lerp(_Steps.x, _Steps.x * 1, abs(ray.y));
		float rayStepLength = 1 * (hitDistance.y - hitDistance.x) / steps;
		float3 rayStep = ray * rayStepLength;
		//float offset = tex2D(_bayerNoise, i.position.xy % 4).r * 5;
		//float3 pos = EyePosition + (hitDistance.x + 0.5 * rayStepLength) * (ray * offset);
		float3 pos = EyePosition + (hitDistance.x + 0.5 * rayStepLength) * ray;
		int executeSteps = min(steps, (int)_Steps.y); 
	
		float extinct = 1.0;
		float opticalDepth = 0.0;
		float cloudDensity = 0.0;

		// Reduce executeSteps when rendering behind objects.
		if (dpth < 1)
			executeSteps *= _stepsInDepth;


		//Raymarching
		[loop]
		for (int i = 0; i < executeSteps; i++)
		{
			float height = calcHeight(pos, pCent);
			//
			//Get out of expensive raymarching
			if (extinct < 0.01 || height > 1.0 || height < 0.0 || _GlobalCoverage <= -0.9)
				break;

			// Get Weather Data
			float3 weather = GetWeather(pos);

			//Check if we are inside of clouds.
			cloudDensity = CalculateCloudDensity(pos, height, weather, ray.y, false);
			
			[branch]
			if (cloudDensity > 0.0)
			{
				float3 ds = GetDensityAlongRay(pos, pCent, LightDirection, weather, ray.y, height);

				cloudDensity = ds.y * _AlphaCoef;
				float currentOpticalDepth = cloudDensity * rayStepLength  * normalize(distance(pos, EyePosition));

				float3 sunLight = pow(LightColor, 2) * ds.x;

				float3 ambientLight = CalculateAmbientLighting(height, _AmbientLightIntensity,sky) * ds.z;

				float hg = PhaseHenyeyGreenStein(inScatteringAngle, _HgPhaseFactor);
				float energy = GetLightEnergy(pos, height, cloudDensity, ds.x, hg, dot(pos, LightDirection), rayStepLength, _ExtinctionCoef);

				ambientLight *= _AmbientLightIntensity * 0.02 * currentOpticalDepth;
				sunLight.rgb = sunLight.rgb * energy * ds.z;
				sunLight.rgb *= _SunLightIntensity * currentOpticalDepth;

				opticalDepth += currentOpticalDepth;
				extinct = Beer(opticalDepth);

				color.rgb += (sunLight.rgb + ambientLight) * extinct;
			
			}			
			pos += rayStep;
		}

		//Calculate Alpha color.a = 1.0 - GetAlpha(opticalDepth * ray.y);
		color.a = 1.0 - GetAlpha(opticalDepth);

		// Merge with sky		
		color = color + float4(sky.rgb, 0) * (1 - (color.a * _SkyBlending));

		//Tonemapping
		if (_Tonemapping == 0)
			color.rgb = saturate(1.0 - exp(-_CloudsExposure * color.rgb));

#if defined(UNITY_COLORSPACE_GAMMA)
		color.rgb = LinearToGammaSpace(color.rgb);
#endif
	
		return color;
	}
		ENDCG
	}
	}
}
