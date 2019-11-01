Shader "Enviro/Blit" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudsTex ("Clouds (RGB)", 2D) = "white" {}
	}
	SubShader 
	{	
		Pass
		{
			Cull Off 
			ZWrite Off
			Ztest LEqual 
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma exclude_renderers gles 

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_ST;
			uniform half4 _MainTex_TexelSize;
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			uniform half4 _CameraDepthTexture_ST;

			uniform sampler2D _SubFrame;
			uniform half4 _SubFrame_ST;
			uniform sampler2D _PrevFrame;
			uniform half4 _PrevFrame_ST;

			uniform float4x4 _Projection;
			uniform float4x4 _ProjectionSPVR;
			uniform float4x4 _InverseProjection;
			uniform float4x4 _InverseProjectionSPVR;

			uniform float4x4 _InverseRotation;
			uniform float4x4 _InverseRotationSPVR;

			uniform float4x4 _PreviousRotation;
			uniform float4x4 _PreviousRotationSPVR;
			
			uniform float _FrameNumber;
			uniform float _ReprojectionPixelSize;

			uniform float2 _SubFrameDimension;
			uniform float2 _FrameDimension;


			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			   float2 uv1 : TEXCOORD1;
			};
			
			v2f vert(appdata_img v)
			{
			   	v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
			#if UNITY_UV_STARTS_AT_TOP
				o.uv1 = v.texcoord.xy;
				if (_MainTex_TexelSize.y < 0)
				o.uv1.y = 1-o.uv1.y;
			#endif	

			   	return o;
			}
			
			float4 frag (v2f i) : COLOR
			{
			#if UNITY_UV_STARTS_AT_TOP
				float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _CameraDepthTexture_ST));
			#else
				float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _CameraDepthTexture_ST));
			#endif

			depthSample = Linear01Depth (depthSample);
			
			#if UNITY_UV_STARTS_AT_TOP
				float4 main = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv1, _MainTex_ST));
			#else
				float4 main = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
			#endif

			float4 final = main;

			if(depthSample > 0.9999)
			{
				float2 uv = floor(i.uv * _FrameDimension);
				float2 uv2 = (floor(i.uv * _SubFrameDimension) + 0.5) / _SubFrameDimension;

				float x = fmod(uv.x, _ReprojectionPixelSize);
				float y = fmod(uv.y, _ReprojectionPixelSize);
				float currentFrame = y * _ReprojectionPixelSize + x;
				
				float4 cloud;

				if (currentFrame == _FrameNumber)
				{
					cloud = tex2D(_SubFrame, UnityStereoScreenSpaceUVAdjust(uv2, _MainTex_ST));
				}
				else
				{
					float4 reprojection;
					float4 pos = float4(i.uv * 2.0 - 1.0, 1.0, 1.0);

#if UNITY_SINGLE_PASS_STEREO
					if (unity_StereoEyeIndex == 0)
					{
						pos = mul(_InverseProjection, pos);
						pos = pos / pos.w;
						pos.xyz = mul((float3x3)_InverseRotation, pos.xyz);
						pos.xyz = mul((float3x3)_PreviousRotation, pos.xyz);

						reprojection = mul(_Projection, pos);
					}
					else
					{
						pos = mul(_InverseProjectionSPVR, pos);
						pos = pos / pos.w;
						pos.xyz = mul((float3x3)_InverseRotationSPVR, pos.xyz);
						pos.xyz = mul((float3x3)_PreviousRotationSPVR, pos.xyz);

						reprojection = mul(_ProjectionSPVR, pos);
					}
#else
					pos = mul(_InverseProjection, pos);
					pos = pos / pos.w;
					pos.xyz = mul((float3x3)_InverseRotation, pos.xyz);
					pos.xyz = mul((float3x3)_PreviousRotation, pos.xyz);

					reprojection = mul(_Projection, pos);
#endif
				
				//	reprojection = reprojection / reprojection.w;
				//	reprojection.xy = reprojection.xy * 0.5 + 0.5;

					if (reprojection.y < 0.0 || reprojection.y > 1.0 || reprojection.x < 0.0 || reprojection.x > 1.0)
					{
						cloud = tex2D(_SubFrame, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
					}
					else
					{
						cloud = tex2D(_PrevFrame, UnityStereoScreenSpaceUVAdjust(reprojection.xy, _MainTex_ST));
					}
				}
				final = lerp(main,cloud,cloud.a);
			}

			return final;
			}
			
			ENDCG
		}
	} 
}
