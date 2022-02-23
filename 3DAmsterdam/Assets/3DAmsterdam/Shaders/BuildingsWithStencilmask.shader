Shader "Universal Render Pipeline/Netherlands3D/Buildings_Stencilmask"
{
    Properties
    {
        _BaseColor("_BaseColor", Color) = (1, 1, 1, 1)
        _ClippingMask("_ClippingMask", Vector) = (0, 0, 0, 0)
        _Size("_Size", Vector) = (0, 0, 0, 0)
        [NoScaleOffset]_MaskMap("_MaskMap", 2D) = "white" {}
        _Stencilmask("Stencilmask", Range(0, 255)) = 0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue" = "AlphaTest"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
    Stencil
        {
        Ref [_Stencilmask]
        Comp Always
        Pass Replace
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile_fog
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
    #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
    #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
    #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float4 color;
        float3 viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        float2 lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 sh;
        #endif
        float4 fogFactorAndVertexLight;
        float4 shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        float3 interp4 : TEXCOORD4;
        #if defined(LIGHTMAP_ON)
        float2 interp5 : TEXCOORD5;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp6 : TEXCOORD6;
        #endif
        float4 interp7 : TEXCOORD7;
        float4 interp8 : TEXCOORD8;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.color;
        output.interp4.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp5.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp6.xyz = input.sh;
        #endif
        output.interp7.xyzw = input.fogFactorAndVertexLight;
        output.interp8.xyzw = input.shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.color = input.interp3.xyzw;
        output.viewDirectionWS = input.interp4.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp5.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp6.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp7.xyzw;
        output.shadowCoord = input.interp8.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    float3 NormalTS;
    half3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = half3(0, 0, 0);
    surface.Metallic = 0;
    surface.Smoothness = 0.5;
    surface.Occlusion = 1;
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "GBuffer"
    Tags
    {
        "LightMode" = "UniversalGBuffer"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile_fog
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
    #pragma multi_compile _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float4 color;
        float3 viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        float2 lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 sh;
        #endif
        float4 fogFactorAndVertexLight;
        float4 shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        float3 interp4 : TEXCOORD4;
        #if defined(LIGHTMAP_ON)
        float2 interp5 : TEXCOORD5;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp6 : TEXCOORD6;
        #endif
        float4 interp7 : TEXCOORD7;
        float4 interp8 : TEXCOORD8;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.color;
        output.interp4.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp5.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp6.xyz = input.sh;
        #endif
        output.interp7.xyzw = input.fogFactorAndVertexLight;
        output.interp8.xyzw = input.shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.color = input.interp3.xyzw;
        output.viewDirectionWS = input.interp4.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp5.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp6.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp7.xyzw;
        output.shadowCoord = input.interp8.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    float3 NormalTS;
    half3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = half3(0, 0, 0);
    surface.Metallic = 0;
    surface.Smoothness = 0.5;
    surface.Occlusion = 1;
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "ShadowCaster"
    Tags
    {
        "LightMode" = "ShadowCaster"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthOnly"
    Tags
    {
        "LightMode" = "DepthOnly"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthNormals"
    Tags
    {
        "LightMode" = "DepthNormals"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.color = input.interp3.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 NormalTS;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "Meta"
    Tags
    {
        "LightMode" = "Meta"
    }

        // Render State
        Cull Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 uv2 : TEXCOORD2;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    half3 Emission;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.Emission = half3(0, 0, 0);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

    ENDHLSL
}
Pass
{
        // Name: <None>
        Tags
        {
            "LightMode" = "Universal2D"
        }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

    ENDHLSL
}
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue" = "AlphaTest"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma multi_compile_fog
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
    #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
    #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
    #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float4 color;
        float3 viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        float2 lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 sh;
        #endif
        float4 fogFactorAndVertexLight;
        float4 shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        float3 interp4 : TEXCOORD4;
        #if defined(LIGHTMAP_ON)
        float2 interp5 : TEXCOORD5;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp6 : TEXCOORD6;
        #endif
        float4 interp7 : TEXCOORD7;
        float4 interp8 : TEXCOORD8;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.color;
        output.interp4.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp5.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp6.xyz = input.sh;
        #endif
        output.interp7.xyzw = input.fogFactorAndVertexLight;
        output.interp8.xyzw = input.shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.color = input.interp3.xyzw;
        output.viewDirectionWS = input.interp4.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp5.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp6.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp7.xyzw;
        output.shadowCoord = input.interp8.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    float3 NormalTS;
    half3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = half3(0, 0, 0);
    surface.Metallic = 0;
    surface.Smoothness = 0.5;
    surface.Occlusion = 1;
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "ShadowCaster"
    Tags
    {
        "LightMode" = "ShadowCaster"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthOnly"
    Tags
    {
        "LightMode" = "DepthOnly"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthNormals"
    Tags
    {
        "LightMode" = "DepthNormals"
    }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.color = input.interp3.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 NormalTS;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "Meta"
    Tags
    {
        "LightMode" = "Meta"
    }

        // Render State
        Cull Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 uv2 : TEXCOORD2;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    half3 Emission;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.Emission = half3(0, 0, 0);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

    ENDHLSL
}
Pass
{
        // Name: <None>
        Tags
        {
            "LightMode" = "Universal2D"
        }

        // Render State
        Cull Back
    Blend One Zero
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _AlphaClip 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 color;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
        float4 VertexColor;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.color = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
half4 _BaseColor;
half4 _ClippingMask;
half2 _Size;
float4 _MaskMap_TexelSize;
half _Stencilmask;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

// Graph Functions

void Unity_Multiply_half(half4 A, half4 B, out half4 Out)
{
    Out = A * B;
}

void Unity_Comparison_Less_half(half A, half B, out half Out)
{
    Out = A < B ? 1 : 0;
}

void Unity_Multiply_half(half A, half B, out half Out)
{
    Out = A * B;
}

void Unity_Dither_half(half In, half4 ScreenPosition, out half Out)
{
    half2 uv = ScreenPosition.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    Out = In - DITHER_THRESHOLDS[index];
}

void Unity_Branch_half(half Predicate, half True, half False, out half Out)
{
    Out = Predicate ? True : False;
}

void Unity_TilingAndOffset_float(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

struct Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7
{
    float3 AbsoluteWorldSpacePosition;
};

void SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(UnityTexture2D Texture2D_E59969F8, float4 Vector4_C27D9F6C, float2 Vector2_9E75FBFC, Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 IN, out float4 OutVector4_1)
{
    UnityTexture2D _Property_dea3cc24e70550849d43fb8887284f97_Out_0 = Texture2D_E59969F8;
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_b92f5ac2b94fea88a27fd34dfeee34c7_A_4 = 0;
    float2 _Vector2_293c240b76c87486b2222de3d94ab277_Out_0 = float2(_Split_b92f5ac2b94fea88a27fd34dfeee34c7_R_1, _Split_b92f5ac2b94fea88a27fd34dfeee34c7_B_3);
    float2 _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0 = Vector2_9E75FBFC;
    float4 _Property_8c5813b5a6f542898f3b9f5511939966_Out_0 = Vector4_C27D9F6C;
    float _Split_7990b0a5c9f4398a926ef238f25e69db_R_1 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[0];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_G_2 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[1];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_B_3 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[2];
    float _Split_7990b0a5c9f4398a926ef238f25e69db_A_4 = _Property_8c5813b5a6f542898f3b9f5511939966_Out_0[3];
    float2 _Vector2_05571ce46584a98dac01053392dc85a0_Out_0 = float2(_Split_7990b0a5c9f4398a926ef238f25e69db_R_1, _Split_7990b0a5c9f4398a926ef238f25e69db_B_3);
    float2 _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3;
    Unity_TilingAndOffset_float(_Vector2_293c240b76c87486b2222de3d94ab277_Out_0, _Property_33a990c1afce0e85be814f75c4b5ab33_Out_0, _Vector2_05571ce46584a98dac01053392dc85a0_Out_0, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float4 _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0 = SAMPLE_TEXTURE2D(_Property_dea3cc24e70550849d43fb8887284f97_Out_0.tex, _Property_dea3cc24e70550849d43fb8887284f97_Out_0.samplerstate, _TilingAndOffset_0125b1dd8f605080b4af1d33155d4a18_Out_3);
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.r;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_G_5 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.g;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_B_6 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.b;
    float _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_A_7 = _SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_RGBA_0.a;
    float4 _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0 = float4(_SampleTexture2D_517eb82e9bfc238abc3ba238df59a7fd_R_4, 1, 1, 1);
    OutVector4_1 = _Vector4_3586c1d0b6952d85a6442e8e52f6d576_Out_0;
}

// Graph Vertex
struct VertexDescription
{
    half3 Position;
    half3 Normal;
    half3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    half3 BaseColor;
    half Alpha;
    half AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half4 _Property_83f5954da7e041758c85c2a77822b2bd_Out_0 = _BaseColor;
    half4 _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2;
    Unity_Multiply_half(_Property_83f5954da7e041758c85c2a77822b2bd_Out_0, IN.VertexColor, _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2);
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_R_1 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[0];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_G_2 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[1];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_B_3 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[2];
    half _Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4 = _Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2[3];
    half _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2;
    Unity_Comparison_Less_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1, _Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2);
    half _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2;
    Unity_Multiply_half(_Split_1c1b748aa74f4de0a39152f5d29bbd40_A_4, 1.5, _Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2);
    half _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2;
    Unity_Dither_half(_Multiply_6b19d2802c0a42b4a66fa89fae228777_Out_2, half4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2);
    half _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3;
    Unity_Branch_half(_Comparison_d80fead655f74e3cbc648bcbe93d7f1f_Out_2, _Dither_8545dd8e251b4d2fbbdcc29ddc1249e3_Out_2, 1, _Branch_242ef354cf25430e9fb670bcac5f7760_Out_3);
    UnityTexture2D _Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0 = UnityBuildTexture2DStructNoScale(_MaskMap);
    half4 _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0 = _ClippingMask;
    half2 _Property_bff7c659185748ca84af527b29dc138e_Out_0 = _Size;
    Bindings_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2;
    _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2.AbsoluteWorldSpacePosition = IN.AbsoluteWorldSpacePosition;
    float4 _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1;
    SG_MaskInWorldSpace_d1589b10477cd884eba3b46e9c796eb7(_Property_66c05abfc4234b41b150d8dd39ce7c43_Out_0, _Property_edf8883a6f6b49469c1fbcbe6d67f474_Out_0, _Property_bff7c659185748ca84af527b29dc138e_Out_0, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2, _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1);
    half4 _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2;
    Unity_Multiply_half((_Branch_242ef354cf25430e9fb670bcac5f7760_Out_3.xxxx), _MaskInWorldSpace_31dc93b03b7846dc86f672f88a9b21f2_OutVector4_1, _Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2);
    surface.BaseColor = (_Multiply_e84b850cb73a4955ba99831a9bb36515_Out_2.xyz);
    surface.Alpha = (_Multiply_29ed45bf04914c6dacee44261a25a11f_Out_2).x;
    surface.AlphaClipThreshold = 0.5;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
    output.VertexColor = input.color;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

    ENDHLSL
}
    }
        CustomEditor "ShaderGraph.PBRMasterGUI"
        FallBack "Hidden/Shader Graph/FallbackError"
}