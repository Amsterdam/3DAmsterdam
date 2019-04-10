// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Enviro/FlatCloudMap" {
	Properties{

	}
		SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma exclude_renderers gles 

		uniform sampler2D _NoiseTex;
		uniform float _Softness = 0.2;
		uniform float _Coverage = 0.6;
		uniform float _Brightness = 1.0;
		uniform float _CloudScale = 2.5;
		uniform float2 _CloudAnimation;
		uniform int noiseOctaves = 8;
		uniform float _MorphingSpeed;
		struct v2f
		{
			float4 Position : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 pos : TEXCOORD1;
		};

		v2f vert(appdata_img v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			o.Position = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}

		float noise(float2 uv)
		{
			return tex2D(_NoiseTex, uv).r;
		}

		float2 rotate(float2 uv)
		{
			float curlStrain = 3.0;
			uv = uv + noise(uv*0.2)*0.005;
			float rot = curlStrain;
			float sinRot = sin(rot);
			float cosRot = cos(rot);
			float2x2 rotMat = float2x2(cosRot, -sinRot, sinRot, cosRot);
			return mul(rotMat,uv);
		}

		float fbm(float2 uv)
		{
			float rot = 1.57;
			float sinRot = sin(rot);
			float cosRot = cos(rot);
			float f = 0.0;
			float total = 0.0;
			float mul = 0.5;
			float2x2 rotMat = float2x2(cosRot, -sinRot, sinRot, cosRot);
			float timeScale = 10.0;
		
			for (int i = 0; i < noiseOctaves; i++)
			{
				f += noise(uv + _Time.y * (_MorphingSpeed * 0.0001) * timeScale * (1.0 - mul)) * mul;
				total += mul;
				uv *= 3.0;
				uv = rotate(uv);
				mul *= 0.5;
			}
			return f / total;
		}

		float4 frag(v2f i) : SV_Target
		{
		 float timeScale = 2.0;
		 float2 uv = i.pos.xy / (20.0 * _CloudScale);


		 float bright = _Brightness;

		 float color1 = fbm(uv - 0.5 + -_CloudAnimation * 0.1);
		 float color2 = fbm(uv - 10.5 + -_CloudAnimation * 0.2);

		 float clouds1 = smoothstep(1.0 - _Coverage, min((1.0 - _Coverage) + _Softness * 2.0, 1.0), color1);
		 float clouds2 = smoothstep(1.0 - _Coverage, min((1.0 - _Coverage) + _Softness, 1.0), color2);

		 float cloudsFormComb = saturate(clouds1 + clouds2);

		 float4 skyCol = float4(1,1,1,0.0);
		 float cloudCol = saturate(saturate(1.0 - pow(color1, 1.0) * 0.2) * bright);
		 float cloudCol2 = saturate(saturate(1.0 - pow(color2, 1.0) * 0.5) * bright);
		 
		 float4 clouds1Color = float4(cloudCol, cloudCol, cloudCol, cloudsFormComb);
		 float4 clouds2Color = lerp(clouds1Color, float4(cloudCol2, cloudCol2, cloudCol2, cloudCol2), clouds2);
		 float4 cloudColComb = lerp(clouds1Color, clouds2Color, saturate(clouds2 - clouds1));

		 float4 final = lerp(skyCol, cloudColComb, cloudsFormComb);

		 return float4(final.rgb, cloudsFormComb);
		}

	ENDCG
	}
	}
	FallBack "Diffuse"
}
