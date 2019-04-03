Shader "EnviroLight/Clouds Particles Advanced" {
Properties {
	_CloudsColor("Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_particles
			#pragma multi_compile_fog
			#pragma multi_compile __ ENVIRO_SIMPLE_FOG
			#pragma multi_compile __ ENVIROVOLUMELIGHT
			#include "UnityCG.cginc"

	#if ENVIRO_SIMPLE_FOG		
		uniform half _SkyFogHeight;
		uniform half _skyFogIntensity;
		uniform float3 _SunDir;
	#else
		#include "Core/EnviroFogCore.cginc"
	#endif
		
			uniform sampler2D _MainTex;
			uniform fixed4 _CloudsColor;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float3 posWorld : TEXCOORD2;
				float4 uv : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv = ComputeScreenPos(o.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			

			fixed4 TransparentParticleCloudsFog(fixed4 clr, float3 wPos, float2 uv)
			{
				float3 wsDir = wPos - _WorldSpaceCameraPos;

				float f = saturate((_SkyFogHeight * dot(normalize(wPos - _WorldSpaceCameraPos.xyz), float3(0, 1, 0))));
				f = pow(f, _skyFogIntensity);
				half fogFacSky = (clamp(f, 0, 1));

				fixed4 fogClr = fixed4(0, 0, 0, 0);
				fixed4 final = fixed4(0, 0, 0, 0);

#if ENVIRO_SIMPLE_FOG
				fogClr = unity_FogColor * 2;
				final = lerp(fogClr, clr, fogFacSky);
#else
				float2 sunDir;
				sunDir.x = saturate(_SunDir.y + 0.25);
				sunDir.y = saturate(clamp(1.0 - _SunDir.y, 0.0, 0.5));
				fogClr = ComputeScattering(normalize(wsDir), sunDir);

	#if ENVIROVOLUMELIGHT
				float4 volumeLighting = tex2D(_EnviroVolumeLightingTex, UnityStereoTransformScreenSpaceTex(uv));
				volumeLighting *= _EnviroParams.x;
				final = lerp(lerp(fogClr, fogClr + volumeLighting, _EnviroVolumeDensity), lerp(clr, clr + volumeLighting, _EnviroVolumeDensity), fogFacSky);
	#else
				final = lerp(fogClr, clr, fogFacSky);
	#endif
#endif

				return final;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = 2.0f * i.color * tex2D(_MainTex, i.texcoord) * _CloudsColor;
				UNITY_APPLY_FOG(i.fogCoord, col);				
				float3 wsDir = normalize(i.posWorld.xyz -_WorldSpaceCameraPos);
				float4 fog = TransparentParticleCloudsFog(col, i.posWorld,i.uv.xy / i.uv.w);
				return float4(fog.rgb, clamp(col.a * clamp(wsDir.y*1.5, 0, 1),0,1));
			}
			ENDCG 
		}
	}	
}
}
