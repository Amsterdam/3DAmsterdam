Shader "Custom/GEbouwenHighlight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_OverrideColor("Highlight", Color) = (1,0,0,1)
		_HighlightTex("Highlight (RGB)", 2D) = "red" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
	
		_OverrideUVx("OverrideUVx",float) = 0.625


    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

		int _SegmentsCount = 0;
		float _pandcodes[1000];
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _HighlightTex;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv2_HighlightTex;

        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed4 _OverrideColor;
		float _OverrideUVx;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
		{
			bool override = false;
			for (int i = 0; i < _SegmentsCount; i++) {
				if (_pandcodes[i] == IN.uv2_HighlightTex.x)
				{
					override = true;
				}
			}
			 //Albedo comes from a texture tinted by color
			if (override)
			{
			fixed4 c = _OverrideColor;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			}
			else
			{

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			}
			override = false;
			
		}
        ENDCG
    }
    FallBack "Diffuse"
}
