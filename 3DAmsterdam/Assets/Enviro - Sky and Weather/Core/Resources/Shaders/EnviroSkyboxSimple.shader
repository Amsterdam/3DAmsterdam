// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Enviro/SkyboxSimple"
{
    Properties
    {
     	_SkyColor ("Sky Color", Color) = (0, 0, 0, 0)
     	_HorizonColor ("Horizon Color", Color) = (0, 0, 0, 0)
        _SunColor ("Sun Color", Color) = (0, 0, 0, 0)
		_Stars ("StarsMap", Cube) = "white" {}
    }
	
    SubShader
    {
		Lod 300
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "IgnoreProjector"="True" }
		
        Pass
        {
            Cull Back
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			#pragma target 2.0 
			//#pragma multi_compile_fog

			uniform half4 _SkyColor;
			uniform half4 _HorizonColor;
			uniform half4 _SunColor;
			uniform samplerCUBE _Stars;
			uniform float4x4 _StarsMatrix;
			uniform half _StarsIntensity;
			uniform half _SunDiskSizeSimple;
			uniform float4 _weatherSkyMod;

			uniform float3 _SunDir;
		  
			struct VertexInput 
             {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
             };


            struct v2f {
                float4 position : POSITION;
                float4 WorldPosition : TEXCOORD0;
                float3 starPos : TEXCOORD1;
				half3 vertex : TEXCOORD2;
            };

            v2f vert(VertexInput v) {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.position  = UnityObjectToClipPos(v.vertex);
				o.WorldPosition = normalize(mul((float4x4)unity_ObjectToWorld, v.vertex)).xyzw;
				o.starPos = mul((float3x3)_StarsMatrix,v.vertex.xyz);
				o.vertex = -v.vertex;
                return o;
            }

			half getMiePhase(half eyeCos, half eyeCos2, half y)
			{
				half temp = 1.0 + 0.9801 - 2.0 * (-0.990) * eyeCos;
				temp = pow(temp, pow(_SunDiskSizeSimple, 0.65) * 10);
				temp = max(temp, 1.0e-4); // prevent division by zero, esp. in half precision
				temp = 1.5 * ((1.0 - 0.9801) / (2.0 + 0.9801)) * (1.0 + eyeCos2) / temp;
//#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
//				temp = pow(temp, .454545);
//#endif
				return temp;
			}

            fixed4 frag(v2f i) : COLOR 
            {
				half3 ray = normalize(mul((float3x3)unity_ObjectToWorld, i.vertex));
				half y = ray.y / 0.02;
				float4 skyColor = float4(0, 0, 0, 1);
				
				if (y < 5.0)
				{		
            		float3 viewDir = normalize(i.WorldPosition + float3(0,0.2,0));
            		float3 starsMap = texCUBE(_Stars, i.starPos.xyz);
					float4 nightSky = float4(((_StarsIntensity * 50) * starsMap.rgb),1);
					skyColor = lerp(_HorizonColor,_SkyColor,smoothstep(dot(viewDir.y, float3(0,2,0)),0,0.3));
					skyColor = skyColor + (1 - skyColor.a) * nightSky;
			
					half eyeCos = dot(_SunDir, ray);
					half eyeCos2 = eyeCos * eyeCos;
					half mie = getMiePhase(eyeCos, eyeCos2,y);
					skyColor += mie * _SunColor;

					skyColor = lerp(skyColor, (lerp(skyColor, _weatherSkyMod, _weatherSkyMod.a)), _weatherSkyMod.a);	
				}	

                return skyColor;
            }
            ENDCG
        }

		//Cirrus Clouds
			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _CloudMap;
			uniform float _CloudAlpha;
			uniform float _CloudCoverage;
			uniform float _CloudAltitude;
			uniform float4 _CloudColor;
			uniform float _CloudColorPower;
			uniform float2 _CloudAnimation;
			uniform float _CloudAnimationSpeed;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 Position : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.Position = UnityObjectToClipPos(v.vertex);
				o.worldPos = normalize(v.vertex).xyz;
				float3 viewDir = normalize(o.worldPos + float3(0,1,0));
				o.worldPos.y *= 1 - dot(viewDir.y + _CloudAltitude, float3(0,-0.15,0));
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 uvs = normalize(i.worldPos);

				float4 uv1;
				float4 uv2;

				uv1.xy = (uvs.xz * 0.2) + _CloudAnimation;
				uv2.xy = (uvs.xz * 0.4) + _CloudAnimation;

				float4 clouds1 = tex2D(_CloudMap, uv1.xy);
				float4 clouds2 = tex2D(_CloudMap, uv2.xy);

				float color1 = pow(clouds1.g + clouds2.g, 0.1);
				float color2 = pow(clouds2.b * clouds1.r, 0.2);

				float4 finalClouds = lerp(clouds1, clouds2, color1 * color2);
				float cloudExtinction = pow(uvs.y , 2);


				finalClouds.a *= _CloudAlpha;
				finalClouds.a *= cloudExtinction;

				if (uvs.y < 0)
					finalClouds.a = 0;

				finalClouds.rgb = finalClouds.a * pow(_CloudColor,_CloudColorPower);
				finalClouds.rgb = pow(finalClouds.rgb,1 - _CloudCoverage);

				return finalClouds;
			}
				ENDCG
			}
    }
    FallBack "None"
}
