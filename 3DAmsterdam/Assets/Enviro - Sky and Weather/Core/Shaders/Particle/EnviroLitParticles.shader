Shader "Enviro/Particles/Lit Weather" {
	Properties{
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_MainTex("Particle Texture", 2D) = "white" {}

	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard alpha:fade finalcolor:ApplyFog
		#pragma target 3.0

		sampler2D _MainTex;
#include "../../Resources/Shaders/Core/EnviroFogCore.cginc"
	struct Input {
		float2 uv_MainTex;
		fixed4 color : COLOR;
		float4 screenPos;
		float3 worldPos;
	};

	fixed4 _TintColor;

	void ApplyFog(Input IN, SurfaceOutputStandard o, inout fixed4 color)
	{
		//Get ScreenPosition
		float3 uvscreen = IN.screenPos.xyz / IN.screenPos.w;
		// Calculate Linear Depth
		half linear01Depth = Linear01Depth(uvscreen.z);
		//get World Position
		float3 wPos = IN.worldPos.xyz;
		// Calculate Fog and apply volume lighting tex
		float4 fogClr = TransparentFog(color, wPos, uvscreen.xy, linear01Depth);

#if _ALPHAPREMULTIPLY_ON
		fogClr.rgb *= o.Alpha;
#endif

#ifndef UNITY_PASS_FORWARDADD
		color.rgb = fogClr.rgb;
#endif
	}

	void surf(Input IN, inout SurfaceOutputStandard o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * (IN.color * _TintColor);
		o.Albedo = c.rgb * 10;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}