Shader "Custom/InstancedTreeShader" {
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
    }

    SubShader{
        Tags { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "AlphaTest"
        }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _BaseColor;
            float4 _Offsets[1023];   // Max instanced batch size.

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                // Allow instancing.
                UNITY_SETUP_INSTANCE_ID(i);

                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.vertex = UnityObjectToClipPos(i.vertex);

                // If instancing on (it should be) assign per-instance uv offset.
                #ifdef UNITY_INSTANCING_ENABLED
                    o.uv = TRANSFORM_TEX(i.uv, _MainTex) + _Offsets[instanceID];
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
            // sample the texture
            fixed4 col = tex2D(_MainTex, i.uv) * _BaseColor;
            //cutout 
            clip(col.a - 0.5);
            // apply fog
            UNITY_APPLY_FOG(i.fogCoord, col);
            return col;
        }
        ENDCG
    }

        Pass {
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Offsets[1023];   // Max instanced batch size.

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                // Allow instancing.
                UNITY_SETUP_INSTANCE_ID(i);

                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.vertex = UnityObjectToClipPos(i.vertex);

                // If instancing on (it should be) assign per-instance uv offset.
                #ifdef UNITY_INSTANCING_ENABLED
                    o.uv = TRANSFORM_TEX(i.uv, _MainTex) + _Offsets[instanceID];
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //cutout 
                clip(col.a - 0.5);
                return col;
            }

            ENDCG
        }
    }
}