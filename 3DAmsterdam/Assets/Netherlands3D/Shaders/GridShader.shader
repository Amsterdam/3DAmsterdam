// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//based

Shader "Unlit/GridOverlay"
{
	Properties
	{
		_GridSize("Grid Size", Float) = 10
		_Grid2Size("Grid 2 Size", Float) = 160
		_Alpha("Alpha", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZTest Always		

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off Lighting Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _GridSize;
			float _Grid2Size;
			float _Alpha;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = mul(unity_ObjectToWorld, v.vertex).xz;
				return o;
			}

			float DrawGrid(float2 uv, float size, float fade)
			{
				float fadeThreshold = fade;
				float fadeMinimum = fade * 0.1;
				float2 gUV = uv / size + fadeThreshold;
				float2 fl = floor(gUV);

				gUV = frac(gUV);
				gUV -= fadeThreshold;
				gUV = smoothstep(fadeThreshold, fadeMinimum, abs(gUV));
				float d = max(gUV.x, gUV.y);

				return d;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed smallGrid = DrawGrid(i.uv, _GridSize, 0.03);
				fixed largeGrid = DrawGrid(i.uv, _Grid2Size, 0.003);

				return float4(1- largeGrid, 1 - largeGrid/2.0, 1, smallGrid * (_Alpha * (i.vertex.z / _WorldSpaceCameraPos.z)));
			}
			ENDCG
		}
	}
}