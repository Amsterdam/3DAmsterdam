Shader "Enviro/MoonShader"
{
    Properties
    {
        _MainTex("Texture (RGB)", 2D) = "black" {}
        _Color("Color", Color) = (0.8, 0.8, 0.8, 1)
        _Brightness("Brightness", Float) = 5
    }
 
	SubShader
    {
		Tags
       	{
         "Queue"="Transparent" 
         "RenderType"="Transparent"
         "IgnoreProjector"="True"
        }
        Pass
        {
           		Cull Back
           		Blend One One
           		
            	CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
				#pragma target 2.0
	
                #include "UnityCG.cginc"
 				uniform float4 _SunPosition;
 				uniform float4 _MoonPosition;
 
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform float4 _Color;
                uniform float _Brightness;
 
                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 normal : TEXCOORD0;
                    float3 worldvertpos : TEXCOORD1;
                    float2 texcoord : TEXCOORD2;
                };
 
                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos (v.vertex);
                    o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                    o.worldvertpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
                }
 
                float4 frag(v2f i) : COLOR
                {

                float3 sunPos = _SunPosition;
                float3 moonPos = _MoonPosition;

                float3 lightVector = normalize(_SunPosition - moonPos);

                    i.normal = normalize(i.normal);
  
                    float3 clr = tex2D(_MainTex, i.texcoord) * _Color;
					clr = pow(clr, 0.3);
                    float d = saturate(max(0.0,dot(i.normal,lightVector)) * 2);
                 	clr = (clr * d) * _Brightness;

                    return float4(clr,1);
                }
            ENDCG
        }
 }}