
Shader "Enviro/LW Version/EnviroFogRenderingSimple" 
{
	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 2.0

	#include "UnityCG.cginc" 

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
	
	uniform float4x4 _LeftWorldFromView;
	uniform float4x4 _RightWorldFromView;
	uniform float4x4 _LeftViewFromScreen;
	uniform float4x4 _RightViewFromScreen;
	uniform float4 _EnviroParams;
	uniform float4 _DistanceParams;
	uniform int4 _SceneFogMode;
	uniform float4 _SceneFogParams;
	uniform half _distanceFogIntensity;
	uniform half _skyFogIntensity;
	uniform half _SkyFogHeight;
	uniform float _maximumFogDensity;
	uniform float _lightning;
	uniform float4 _bottomFogColor;

	struct appdata_t 
	{
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f 
	{
		float4 pos : SV_POSITION;
		float3 texcoord : TEXCOORD0;
		float2 uv : TEXCOORD1;
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
		return o;
	}

	half ComputeFogFactor(float coord)
	{
		float fogFac = 0.0;

		if (_SceneFogMode.x == 1) // linear
		{
			fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
		}

		if (_SceneFogMode.x >= 2) // exp
		{
			fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
		}

		return saturate(fogFac);
	}

	// Distance fog
	float ComputeDistance(float3 camDir, float zdepth)
	{
		float dist;
		dist = length(camDir);
		dist -= _ProjectionParams.y;
		return dist;
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

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		half fogFac = 0;
		float4 finalFog = unity_FogColor;
		float g = _DistanceParams.x;
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Scene
		if (dpth < 0.99999) 
		{
			// Calculate Distance Fog
			if (_EnviroParams.y > 0)
			{
				g += ComputeDistance(wsDir,dpth);
				g *= _distanceFogIntensity;
			}

			// Compute fog amount
			fogFac = ComputeFogFactor(max(0.0, g));
			fogFac = lerp(_maximumFogDensity, 1.0f, fogFac);
		}
		else //SKY
		{
			//float3 pos = wsPos - float3(0, -1000, 0);
			float3 viewDir = normalize(wsDir);
			float f = saturate(_SkyFogHeight * viewDir.y);
			//float f = smoothstep(dot(viewDir.y, float3(0, 2, 0)), 0, 0.3);
			//float f = saturate((_SkyFogHeight * dot(normalize(wsDir.xyz), float3(0, 1, 0))));
			//f = pow(f, _skyFogIntensity);
			fogFac = (clamp(f, _skyFogIntensity, 1));
			//fogFac = 1;
		}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		float4 source = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));		
		return lerp (finalFog, source, fogFac);
		}
		ENDCG
		}
	}
	Fallback Off
}
