
Shader "Enviro/LW Version/EnviroFogRendering" 
{
	Properties
	{ 
		//_Source("Source",  2D) = "black"{}
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

		//  Start: LuxWater
#pragma multi_compile __ LUXWATER_DEFERREDFOG

#if defined(LUXWATER_DEFERREDFOG)
		sampler2D _UnderWaterMask;
	float4 _LuxUnderWaterDeferredFogParams; // x: IsInsideWatervolume?, y: BelowWaterSurface shift, z: EdgeBlend
#endif
											//  End: LuxWater


	#include "UnityCG.cginc" 
	#include "../../../Core/Resources/Shaders/Core/EnviroFogCore.cginc"

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
	uniform float4x4 _LeftWorldFromView;
	uniform float4x4 _RightWorldFromView;
	uniform float4x4 _LeftViewFromScreen;
	uniform float4x4 _RightViewFromScreen;

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
		float2 uv : TEXCOORD2;
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
		half gAdd = 0;

		if (_EnviroParams.z > 0)
		{
			gAdd = ComputeHalfSpace (wsDir);
		} 

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

			// AAdd Height Fog
			g += gAdd;

			// Compute fog amount
			fogFac = ComputeFogFactor(max(0.0, g));
			fogFac = lerp(_maximumFogDensity, 1.0f, fogFac);

			finalFog = ComputeScatteringScene(viewDir, i.sky.xy);

		}
		else //SKY
		{
			//float f = saturate((_SkyFogHeight * dot(normalize(wsPos - _WorldSpaceCameraPos.xyz), float3(0, 1, 0))));
			float f = saturate(_SkyFogHeight * (viewDir.y + 0.05));
			//fogFac = pow(f, _skyFogIntensity);
			fogFac = (clamp(f, _skyFogIntensity, 1));
			finalFog = ComputeScatteringScene(viewDir, i.sky.xy);

		}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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



		float4 source = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));		
		return lerp (finalFog, source, fogFac);
		}
		ENDCG
		}
	}
	Fallback Off
}
