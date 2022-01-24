Shader "Unlit/dataShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_GradientTex("Gradient for data", 2D) = "white" {}
        //_minValue("minValue", Float) = 0
        //_maxValue("maxValue", Float) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Geometry" 
            "Queue"="AlphaTest+5"
            
        }

        LOD 100
        
        ZTest LEqual
        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        //Blend One OneMinusSrcAlpha // Premultiplied transparency
        //Blend One One // Additive
     //   Blend OneMinusDstColor One // Soft additive
        //Blend DstColor Zero // Multiplicative
        //Blend DstColor SrcColor // 2x multiplicative

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _minValue;
            float _maxValue;
            sampler2D _HeatGradientTex;

            v2f vert (appdata v)
            {
                v2f o;
                //v.vertex.y += 8.0f;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // [TODO] Preferably 16 bit instead of 8 bit textures.
                float value = tex2D(_MainTex, i.uv).r;

                // We sample gradient based on this value. Value is clamped (0-1).
                float normalized = saturate((value - _minValue) / (_maxValue - _minValue));
                fixed4 col = tex2D(_HeatGradientTex, float2(normalized, 0.5f));
                
                col.a = 1; // saturate(normalized * 0.5f);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
