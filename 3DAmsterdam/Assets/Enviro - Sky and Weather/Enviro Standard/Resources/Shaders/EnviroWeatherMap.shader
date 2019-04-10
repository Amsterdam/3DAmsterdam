Shader "Enviro/WeatherMap" {
	Properties {
		_Coverage ("Coverage", Range(0,1)) = 0.5
		_Tiling ("Tiling", Range(1,100)) = 10
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass {
		CGPROGRAM
	    #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        #include "/Core/EnviroNoiseCore.cginc"
		#pragma target 3.0
		#pragma exclude_renderers gles 

		sampler2D _MainTex;

		   struct VertexInput {
  				half4 vertex : POSITION;
 				float2 uv : TEXCOORD0;	
            };

            struct VertexOutput {
           		float4 position : SV_POSITION;
 				float2 uv : TEXCOORD0;
            };

            VertexOutput vert (VertexInput v) {
 			 	VertexOutput o;
 				o.position = UnityObjectToClipPos(v.vertex);				
 				o.uv = v.uv;
 				return o;
            }
 		
 			float4x4 world_view_proj;

 			float _Coverage;
 			int _Tiling;
 			float2 _WindDir;
			float2 _Location;
 			float _AnimSpeedScale;

			float set_range(float value, float low, float high) {
							return saturate((value - low)/(high - low));
			}

			float remap(float value, float original_min, float original_max, float new_min, float new_max)
			{
  			  return new_min + saturate(((value - original_min) / (original_max - original_min)) * (new_max - new_min));
			}

		

 			float4 frag(VertexInput input) : SV_Target 
 			{
				float2 xy_offset = _WindDir * 10 * _AnimSpeedScale;
 				float2 xy_offset1 = xy_offset;
				float2 xy_offset2 = xy_offset;

 				float z_offset1 = 0.0;
				float z_offset4 = 200.0;

 				float3 sampling_pos1 = float3(input.uv + xy_offset1 + _Location, z_offset1) * 2.0 * _Tiling;
				float3 sampling_pos2 = float3(input.uv + xy_offset1 + _Location, z_offset1) * 2.0 * _Tiling;
				float3 sampling_pos3 = float3(input.uv + xy_offset1 + _Location, z_offset4) * 6.0 * _Tiling;

				float perlin = perlin5oct(sampling_pos1);
				perlin = pow(perlin * 2, 1.0);
				perlin = perlin + _Coverage; 
				perlin = clamp(perlin, 0, 1);
			
				float perlin3 = perlin5oct(sampling_pos3);			
				//perlin3 = pow(perlin3 * 2, 4);
				perlin3 = clamp(perlin3, 0.0, 0.5);
				
				float perlin2 = perlin5oct(sampling_pos2);
				perlin2 = pow(perlin2, 1);			
				//perlin2 += perlin3;
				perlin2 = perlin2 + _Coverage;
				perlin2 = clamp(perlin2, 0, 1);


				float h = pow(perlin, 3);				
				h -= perlin3;

				h = clamp(h * _Coverage, 0.25, 1);

				return float4(perlin, 0, perlin2, 0);



			/*	float z_offset1 = 0.0;
				float z_offset2 = 500.0;
				float z_offset3 = 100.0;
				float z_offset4 = 200.0;

				float3 sampling_pos1 = float3(input.uv + xy_offset1, z_offset1) * 2.0 * _Tiling;
				float3 sampling_pos2 = float3(input.uv + xy_offset2, z_offset2) * 4.0 * _Tiling;
				float3 sampling_pos3 = float3(input.uv + xy_offset2, z_offset3) * 5.5 * _Tiling;
				float3 sampling_pos4 = float3(input.uv + xy_offset2, z_offset4) * 2.0 * _Tiling;

				float signal1 = perlin5oct(sampling_pos1);
				float signal2 = perlin5oct(sampling_pos2);
				float signal3 = perlin5oct(sampling_pos3);
				float signal4 = perlin5oct(sampling_pos4);

				float perlin1 = set_range(signal1, 0.5, 1.3);
				float perlin2 = set_range(signal2, 0.5, 1.3);
				float perlin3 = set_range(signal3, 0.5, 1.3);
				float perlin4 = set_range(signal4, 0.5, 1.3);

				perlin1 = pow(perlin1, 0.75);
				perlin2 = pow(perlin2, 1.00);
				perlin3 = pow(perlin3, 1.00);
				perlin4 = pow(perlin4, 0.50);

				perlin1 = saturate(perlin1 * 1.2) * 0.4 + _Coverage * 5;
				perlin2 = saturate(perlin2 * 1.0) * 0.5;
				perlin3 = saturate(perlin3 * 1.0) * 0.5;
				perlin4 = saturate(1.0 - perlin4 * 2.0);

				perlin1 -= perlin4 * 0.5;
				perlin2 += pow(perlin4, 2.0);
				perlin3 += perlin4;
 
				return float4(perlin1, perlin2, perlin3, perlin4);
				*/
			}

	ENDCG
	}
	}
	FallBack "Diffuse"
}
