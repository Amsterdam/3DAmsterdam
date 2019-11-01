// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Enviro/MoonShaderPhased"
{
    Properties
    {
        _MainTex ("Moon Texture", 2D)    = "white" {}
        _Phase   ("Moon Phase", float) = 0
        _Brightness ("Moon Brightness", Range(0.1,5)) = 0.5
    }

    SubShader
    {
        Tags
        {"Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Fog
        {
            Mode Off
        }

        Pass
        {
            Cull Back
			Blend SrcAlpha OneMinusSrcAlpha

              CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Phase;
            float _Brightness;

            struct v2f {
                float4 position   : POSITION;
                fixed4 color      : COLOR;
                float2 uv_MainTex : TEXCOORD0;
                float3 normal     : TEXCOORD1;
                float3 viewdir    : TEXCOORD2;
            };

			float MoonPhaseFactor(float2 uv, float phase)
			{
				float alpha = 1.0;

				float srefx = uv.x - 0.5;
				float refx = abs(uv.x - 0.5);
				float refy = abs(uv.y - 0.5);
				float refxfory = sqrt(0.25 - refy * refy);
				float xmin = -refxfory;
				float xmax = refxfory;
				float xmin1 = (xmax - xmin) * (phase / 2) + xmin;
				float xmin2 = (xmax - xmin) * phase + xmin;
				if (srefx < xmin1) {
					alpha = 0;
				} else if (srefx < xmin2 && xmin1 != xmin2) {
					alpha = (srefx - xmin1) / (xmin2 - xmin1);
				}
				return alpha;
			}

            v2f vert(appdata_base v) {
                v2f o;

                float phaseabs = abs(_Phase);
                float3 offset = 10 * float3(_Phase, -phaseabs, -phaseabs);

                float3 normal  = v.normal;
                float3 viewdir = normalize(ObjSpaceViewDir(v.vertex));

                o.position   = UnityObjectToClipPos(v.vertex);
                o.color.rgb  = 1 - phaseabs;
                o.color.a    = 1;
                o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal     = v.normal;
                o.viewdir    = normalize(viewdir + offset);

                return o;
            }

            fixed4 frag(v2f i) : COLOR {
                fixed4 color = i.color;

			    float alpha = MoonPhaseFactor(i.uv_MainTex, abs(_Phase));
                fixed shading = max(0, dot(i.normal, i.viewdir));
                color.rgb *= pow(shading, 0.5);

                // Moon texture
                fixed3 moontex = tex2D(_MainTex, i.uv_MainTex);
                color.rgb *= moontex.rgb * 2.5;
				
				float lum = dot(color.rgb, float3(0.8, 0.8, 0.8));
				color.a = min(color.a, lum * alpha);
				color.rgb = saturate(1.0 - exp(-_Brightness * color.rgb));
                return color;
            }

            ENDCG
        }
    }
}
