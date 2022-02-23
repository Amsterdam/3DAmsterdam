Shader "SubGrid"
{
    Properties
    {
        Color_1("_MainGridColor", Color) = (1, 1, 1, 1)
        Color_328ade08c2094a788fa974395f6e561f("_LargeGridColor", Color) = (1, 1, 1, 1)
        Vector1_a906bcbc01034447a774e7157f439325("_MainGridTiling", Float) = 1
        Vector1_a906bcbc01034447a774e7157f439325_1("_LargeGridTiling", Float) = 1
        Vector1_a906bcbc01034447a774e7157f439325_2("_MainGridLineThickness", Float) = 1
        Vector1_a906bcbc01034447a774e7157f439325_3("_LargeGridLineThickness", Float) = 1
        Vector2_3bb87bf6a4674236a46e2b16cdeb72e9("_FadeFromToDistance", Vector) = (0, 5000, 0, 0)
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue" = "Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
            // LightMode: <None>
        }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest Always
    ZWrite Off

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
        #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma shader_feature _ _SAMPLE_GI
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
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
        float4 uv0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 texCoord0;
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
        float4 uv0;
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
        output.interp1.xyzw = input.texCoord0;
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
        output.texCoord0 = input.interp1.xyzw;
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
half4 Color_1;
half4 Color_328ade08c2094a788fa974395f6e561f;
half Vector1_a906bcbc01034447a774e7157f439325;
half Vector1_a906bcbc01034447a774e7157f439325_1;
half Vector1_a906bcbc01034447a774e7157f439325_2;
half Vector1_a906bcbc01034447a774e7157f439325_3;
half2 Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_half2(half2 In, out half2 Out)
{
    Out = frac(In);
}

void Unity_Rectangle_half(half2 UV, half Width, half Height, out half Out)
{
    half2 d = abs(UV * 2 - 1) - half2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_OneMinus_half(half In, out half Out)
{
    Out = 1 - In;
}

void Unity_Comparison_Greater_half(half A, half B, out half Out)
{
    Out = A > B ? 1 : 0;
}

void Unity_Branch_half4(half Predicate, half4 True, half4 False, out half4 Out)
{
    Out = Predicate ? True : False;
}

void Unity_Blend_Screen_half(half Base, half Blend, out half Out, half Opacity)
{
    Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
    Out = lerp(Base, Out, Opacity);
}

void Unity_Distance_float3(float3 A, float3 B, out float Out)
{
    Out = distance(A, B);
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half _Property_816cd0f020e341689218fdfa135b61f0_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_1;
    half _Float_6a98aa8a551f43e09c01822dab195b73_Out_0 = _Property_816cd0f020e341689218fdfa135b61f0_Out_0;
    half2 _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_6a98aa8a551f43e09c01822dab195b73_Out_0.xx), half2 (0, 0), _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3);
    half2 _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3, _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1);
    half _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_3;
    half _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0 = _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0;
    half _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3;
    Unity_Rectangle_half(_Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3);
    half _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1;
    Unity_OneMinus_half(_Rectangle_3956398494ff4acea9b8910877d3c065_Out_3, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1);
    half _Comparison_c2fe362568a7421997c9f9d96affe0bd_Out_2;
    Unity_Comparison_Greater_half(_OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, 0.1, _Comparison_c2fe362568a7421997c9f9d96affe0bd_Out_2);
    half4 _Property_97128e6b899c41c99fe64525da74777a_Out_0 = Color_328ade08c2094a788fa974395f6e561f;
    half4 _Property_c3f3cfd35b0945d8ae282815d8e7f439_Out_0 = Color_1;
    half4 _Branch_c4635b8e87c148f190b08f5072768496_Out_3;
    Unity_Branch_half4(_Comparison_c2fe362568a7421997c9f9d96affe0bd_Out_2, _Property_97128e6b899c41c99fe64525da74777a_Out_0, _Property_c3f3cfd35b0945d8ae282815d8e7f439_Out_0, _Branch_c4635b8e87c148f190b08f5072768496_Out_3);
    half _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0 = Vector1_a906bcbc01034447a774e7157f439325;
    half _Float_86b90f585d9e419685c9d6129c32ad92_Out_0 = _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0;
    half2 _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_86b90f585d9e419685c9d6129c32ad92_Out_0.xx), half2 (0, 0), _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3);
    half2 _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3, _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1);
    half _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_2;
    half _Float_34d1f47975c24b9d830b7e63338babb2_Out_0 = _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0;
    half _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3;
    Unity_Rectangle_half(_Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3);
    half _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1;
    Unity_OneMinus_half(_Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3, _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1);
    half _Blend_a56c5b0489e24745855fa904b20f4807_Out_2;
    Unity_Blend_Screen_half(_OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, _Blend_a56c5b0489e24745855fa904b20f4807_Out_2, 1);
    float _Distance_c1f3965872c542578a1c3a4012301496_Out_2;
    Unity_Distance_float3(_WorldSpaceCameraPos, IN.WorldSpacePosition, _Distance_c1f3965872c542578a1c3a4012301496_Out_2);
    half2 _Property_c0f9d128ce18446ab3574833920ea240_Out_0 = Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
    float _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3;
    Unity_Remap_float(_Distance_c1f3965872c542578a1c3a4012301496_Out_2, _Property_c0f9d128ce18446ab3574833920ea240_Out_0, float2 (1, 0), _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3);
    float _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
    Unity_Multiply_float(_Blend_a56c5b0489e24745855fa904b20f4807_Out_2, _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3, _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2);
    surface.BaseColor = (_Branch_c4635b8e87c148f190b08f5072768496_Out_3.xyz);
    surface.Alpha = _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
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
    output.uv0 = input.texCoord0;
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
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

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
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
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
        float4 uv0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 texCoord0;
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
        float4 uv0;
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
        output.interp1.xyzw = input.texCoord0;
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
        output.texCoord0 = input.interp1.xyzw;
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
half4 Color_1;
half4 Color_328ade08c2094a788fa974395f6e561f;
half Vector1_a906bcbc01034447a774e7157f439325;
half Vector1_a906bcbc01034447a774e7157f439325_1;
half Vector1_a906bcbc01034447a774e7157f439325_2;
half Vector1_a906bcbc01034447a774e7157f439325_3;
half2 Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_half2(half2 In, out half2 Out)
{
    Out = frac(In);
}

void Unity_Rectangle_half(half2 UV, half Width, half Height, out half Out)
{
    half2 d = abs(UV * 2 - 1) - half2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_OneMinus_half(half In, out half Out)
{
    Out = 1 - In;
}

void Unity_Blend_Screen_half(half Base, half Blend, out half Out, half Opacity)
{
    Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
    Out = lerp(Base, Out, Opacity);
}

void Unity_Distance_float3(float3 A, float3 B, out float Out)
{
    Out = distance(A, B);
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0 = Vector1_a906bcbc01034447a774e7157f439325;
    half _Float_86b90f585d9e419685c9d6129c32ad92_Out_0 = _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0;
    half2 _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_86b90f585d9e419685c9d6129c32ad92_Out_0.xx), half2 (0, 0), _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3);
    half2 _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3, _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1);
    half _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_2;
    half _Float_34d1f47975c24b9d830b7e63338babb2_Out_0 = _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0;
    half _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3;
    Unity_Rectangle_half(_Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3);
    half _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1;
    Unity_OneMinus_half(_Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3, _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1);
    half _Property_816cd0f020e341689218fdfa135b61f0_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_1;
    half _Float_6a98aa8a551f43e09c01822dab195b73_Out_0 = _Property_816cd0f020e341689218fdfa135b61f0_Out_0;
    half2 _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_6a98aa8a551f43e09c01822dab195b73_Out_0.xx), half2 (0, 0), _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3);
    half2 _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3, _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1);
    half _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_3;
    half _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0 = _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0;
    half _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3;
    Unity_Rectangle_half(_Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3);
    half _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1;
    Unity_OneMinus_half(_Rectangle_3956398494ff4acea9b8910877d3c065_Out_3, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1);
    half _Blend_a56c5b0489e24745855fa904b20f4807_Out_2;
    Unity_Blend_Screen_half(_OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, _Blend_a56c5b0489e24745855fa904b20f4807_Out_2, 1);
    float _Distance_c1f3965872c542578a1c3a4012301496_Out_2;
    Unity_Distance_float3(_WorldSpaceCameraPos, IN.WorldSpacePosition, _Distance_c1f3965872c542578a1c3a4012301496_Out_2);
    half2 _Property_c0f9d128ce18446ab3574833920ea240_Out_0 = Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
    float _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3;
    Unity_Remap_float(_Distance_c1f3965872c542578a1c3a4012301496_Out_2, _Property_c0f9d128ce18446ab3574833920ea240_Out_0, float2 (1, 0), _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3);
    float _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
    Unity_Multiply_float(_Blend_a56c5b0489e24745855fa904b20f4807_Out_2, _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3, _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2);
    surface.Alpha = _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
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
    output.uv0 = input.texCoord0;
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
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
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
        float4 uv0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 texCoord0;
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
        float4 uv0;
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
        output.interp1.xyzw = input.texCoord0;
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
        output.texCoord0 = input.interp1.xyzw;
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
half4 Color_1;
half4 Color_328ade08c2094a788fa974395f6e561f;
half Vector1_a906bcbc01034447a774e7157f439325;
half Vector1_a906bcbc01034447a774e7157f439325_1;
half Vector1_a906bcbc01034447a774e7157f439325_2;
half Vector1_a906bcbc01034447a774e7157f439325_3;
half2 Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_half2(half2 In, out half2 Out)
{
    Out = frac(In);
}

void Unity_Rectangle_half(half2 UV, half Width, half Height, out half Out)
{
    half2 d = abs(UV * 2 - 1) - half2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_OneMinus_half(half In, out half Out)
{
    Out = 1 - In;
}

void Unity_Blend_Screen_half(half Base, half Blend, out half Out, half Opacity)
{
    Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
    Out = lerp(Base, Out, Opacity);
}

void Unity_Distance_float3(float3 A, float3 B, out float Out)
{
    Out = distance(A, B);
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0 = Vector1_a906bcbc01034447a774e7157f439325;
    half _Float_86b90f585d9e419685c9d6129c32ad92_Out_0 = _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0;
    half2 _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_86b90f585d9e419685c9d6129c32ad92_Out_0.xx), half2 (0, 0), _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3);
    half2 _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3, _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1);
    half _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_2;
    half _Float_34d1f47975c24b9d830b7e63338babb2_Out_0 = _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0;
    half _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3;
    Unity_Rectangle_half(_Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3);
    half _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1;
    Unity_OneMinus_half(_Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3, _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1);
    half _Property_816cd0f020e341689218fdfa135b61f0_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_1;
    half _Float_6a98aa8a551f43e09c01822dab195b73_Out_0 = _Property_816cd0f020e341689218fdfa135b61f0_Out_0;
    half2 _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_6a98aa8a551f43e09c01822dab195b73_Out_0.xx), half2 (0, 0), _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3);
    half2 _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3, _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1);
    half _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_3;
    half _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0 = _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0;
    half _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3;
    Unity_Rectangle_half(_Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3);
    half _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1;
    Unity_OneMinus_half(_Rectangle_3956398494ff4acea9b8910877d3c065_Out_3, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1);
    half _Blend_a56c5b0489e24745855fa904b20f4807_Out_2;
    Unity_Blend_Screen_half(_OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, _Blend_a56c5b0489e24745855fa904b20f4807_Out_2, 1);
    float _Distance_c1f3965872c542578a1c3a4012301496_Out_2;
    Unity_Distance_float3(_WorldSpaceCameraPos, IN.WorldSpacePosition, _Distance_c1f3965872c542578a1c3a4012301496_Out_2);
    half2 _Property_c0f9d128ce18446ab3574833920ea240_Out_0 = Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
    float _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3;
    Unity_Remap_float(_Distance_c1f3965872c542578a1c3a4012301496_Out_2, _Property_c0f9d128ce18446ab3574833920ea240_Out_0, float2 (1, 0), _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3);
    float _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
    Unity_Multiply_float(_Blend_a56c5b0489e24745855fa904b20f4807_Out_2, _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3, _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2);
    surface.Alpha = _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
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
    output.uv0 = input.texCoord0;
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
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue" = "Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
            // LightMode: <None>
        }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite Off

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
    #pragma shader_feature _ _SAMPLE_GI
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
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
        float4 uv0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 texCoord0;
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
        float4 uv0;
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
        output.interp1.xyzw = input.texCoord0;
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
        output.texCoord0 = input.interp1.xyzw;
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
half4 Color_1;
half4 Color_328ade08c2094a788fa974395f6e561f;
half Vector1_a906bcbc01034447a774e7157f439325;
half Vector1_a906bcbc01034447a774e7157f439325_1;
half Vector1_a906bcbc01034447a774e7157f439325_2;
half Vector1_a906bcbc01034447a774e7157f439325_3;
half2 Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_half2(half2 In, out half2 Out)
{
    Out = frac(In);
}

void Unity_Rectangle_half(half2 UV, half Width, half Height, out half Out)
{
    half2 d = abs(UV * 2 - 1) - half2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_OneMinus_half(half In, out half Out)
{
    Out = 1 - In;
}

void Unity_Comparison_Greater_half(half A, half B, out half Out)
{
    Out = A > B ? 1 : 0;
}

void Unity_Branch_half4(half Predicate, half4 True, half4 False, out half4 Out)
{
    Out = Predicate ? True : False;
}

void Unity_Blend_Screen_half(half Base, half Blend, out half Out, half Opacity)
{
    Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
    Out = lerp(Base, Out, Opacity);
}

void Unity_Distance_float3(float3 A, float3 B, out float Out)
{
    Out = distance(A, B);
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half _Property_816cd0f020e341689218fdfa135b61f0_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_1;
    half _Float_6a98aa8a551f43e09c01822dab195b73_Out_0 = _Property_816cd0f020e341689218fdfa135b61f0_Out_0;
    half2 _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_6a98aa8a551f43e09c01822dab195b73_Out_0.xx), half2 (0, 0), _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3);
    half2 _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3, _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1);
    half _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_3;
    half _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0 = _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0;
    half _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3;
    Unity_Rectangle_half(_Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3);
    half _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1;
    Unity_OneMinus_half(_Rectangle_3956398494ff4acea9b8910877d3c065_Out_3, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1);
    half _Comparison_c2fe362568a7421997c9f9d96affe0bd_Out_2;
    Unity_Comparison_Greater_half(_OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, 0.1, _Comparison_c2fe362568a7421997c9f9d96affe0bd_Out_2);
    half4 _Property_97128e6b899c41c99fe64525da74777a_Out_0 = Color_328ade08c2094a788fa974395f6e561f;
    half4 _Property_c3f3cfd35b0945d8ae282815d8e7f439_Out_0 = Color_1;
    half4 _Branch_c4635b8e87c148f190b08f5072768496_Out_3;
    Unity_Branch_half4(_Comparison_c2fe362568a7421997c9f9d96affe0bd_Out_2, _Property_97128e6b899c41c99fe64525da74777a_Out_0, _Property_c3f3cfd35b0945d8ae282815d8e7f439_Out_0, _Branch_c4635b8e87c148f190b08f5072768496_Out_3);
    half _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0 = Vector1_a906bcbc01034447a774e7157f439325;
    half _Float_86b90f585d9e419685c9d6129c32ad92_Out_0 = _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0;
    half2 _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_86b90f585d9e419685c9d6129c32ad92_Out_0.xx), half2 (0, 0), _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3);
    half2 _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3, _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1);
    half _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_2;
    half _Float_34d1f47975c24b9d830b7e63338babb2_Out_0 = _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0;
    half _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3;
    Unity_Rectangle_half(_Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3);
    half _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1;
    Unity_OneMinus_half(_Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3, _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1);
    half _Blend_a56c5b0489e24745855fa904b20f4807_Out_2;
    Unity_Blend_Screen_half(_OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, _Blend_a56c5b0489e24745855fa904b20f4807_Out_2, 1);
    float _Distance_c1f3965872c542578a1c3a4012301496_Out_2;
    Unity_Distance_float3(_WorldSpaceCameraPos, IN.WorldSpacePosition, _Distance_c1f3965872c542578a1c3a4012301496_Out_2);
    half2 _Property_c0f9d128ce18446ab3574833920ea240_Out_0 = Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
    float _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3;
    Unity_Remap_float(_Distance_c1f3965872c542578a1c3a4012301496_Out_2, _Property_c0f9d128ce18446ab3574833920ea240_Out_0, float2 (1, 0), _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3);
    float _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
    Unity_Multiply_float(_Blend_a56c5b0489e24745855fa904b20f4807_Out_2, _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3, _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2);
    surface.BaseColor = (_Branch_c4635b8e87c148f190b08f5072768496_Out_3.xyz);
    surface.Alpha = _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
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
    output.uv0 = input.texCoord0;
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
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

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
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
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
        float4 uv0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 texCoord0;
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
        float4 uv0;
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
        output.interp1.xyzw = input.texCoord0;
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
        output.texCoord0 = input.interp1.xyzw;
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
half4 Color_1;
half4 Color_328ade08c2094a788fa974395f6e561f;
half Vector1_a906bcbc01034447a774e7157f439325;
half Vector1_a906bcbc01034447a774e7157f439325_1;
half Vector1_a906bcbc01034447a774e7157f439325_2;
half Vector1_a906bcbc01034447a774e7157f439325_3;
half2 Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_half2(half2 In, out half2 Out)
{
    Out = frac(In);
}

void Unity_Rectangle_half(half2 UV, half Width, half Height, out half Out)
{
    half2 d = abs(UV * 2 - 1) - half2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_OneMinus_half(half In, out half Out)
{
    Out = 1 - In;
}

void Unity_Blend_Screen_half(half Base, half Blend, out half Out, half Opacity)
{
    Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
    Out = lerp(Base, Out, Opacity);
}

void Unity_Distance_float3(float3 A, float3 B, out float Out)
{
    Out = distance(A, B);
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0 = Vector1_a906bcbc01034447a774e7157f439325;
    half _Float_86b90f585d9e419685c9d6129c32ad92_Out_0 = _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0;
    half2 _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_86b90f585d9e419685c9d6129c32ad92_Out_0.xx), half2 (0, 0), _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3);
    half2 _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3, _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1);
    half _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_2;
    half _Float_34d1f47975c24b9d830b7e63338babb2_Out_0 = _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0;
    half _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3;
    Unity_Rectangle_half(_Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3);
    half _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1;
    Unity_OneMinus_half(_Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3, _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1);
    half _Property_816cd0f020e341689218fdfa135b61f0_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_1;
    half _Float_6a98aa8a551f43e09c01822dab195b73_Out_0 = _Property_816cd0f020e341689218fdfa135b61f0_Out_0;
    half2 _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_6a98aa8a551f43e09c01822dab195b73_Out_0.xx), half2 (0, 0), _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3);
    half2 _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3, _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1);
    half _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_3;
    half _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0 = _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0;
    half _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3;
    Unity_Rectangle_half(_Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3);
    half _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1;
    Unity_OneMinus_half(_Rectangle_3956398494ff4acea9b8910877d3c065_Out_3, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1);
    half _Blend_a56c5b0489e24745855fa904b20f4807_Out_2;
    Unity_Blend_Screen_half(_OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, _Blend_a56c5b0489e24745855fa904b20f4807_Out_2, 1);
    float _Distance_c1f3965872c542578a1c3a4012301496_Out_2;
    Unity_Distance_float3(_WorldSpaceCameraPos, IN.WorldSpacePosition, _Distance_c1f3965872c542578a1c3a4012301496_Out_2);
    half2 _Property_c0f9d128ce18446ab3574833920ea240_Out_0 = Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
    float _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3;
    Unity_Remap_float(_Distance_c1f3965872c542578a1c3a4012301496_Out_2, _Property_c0f9d128ce18446ab3574833920ea240_Out_0, float2 (1, 0), _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3);
    float _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
    Unity_Multiply_float(_Blend_a56c5b0489e24745855fa904b20f4807_Out_2, _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3, _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2);
    surface.Alpha = _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
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
    output.uv0 = input.texCoord0;
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
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
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
        float4 uv0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float4 texCoord0;
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
        float4 uv0;
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
        output.interp1.xyzw = input.texCoord0;
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
        output.texCoord0 = input.interp1.xyzw;
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
half4 Color_1;
half4 Color_328ade08c2094a788fa974395f6e561f;
half Vector1_a906bcbc01034447a774e7157f439325;
half Vector1_a906bcbc01034447a774e7157f439325_1;
half Vector1_a906bcbc01034447a774e7157f439325_2;
half Vector1_a906bcbc01034447a774e7157f439325_3;
half2 Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_half2(half2 In, out half2 Out)
{
    Out = frac(In);
}

void Unity_Rectangle_half(half2 UV, half Width, half Height, out half Out)
{
    half2 d = abs(UV * 2 - 1) - half2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_OneMinus_half(half In, out half Out)
{
    Out = 1 - In;
}

void Unity_Blend_Screen_half(half Base, half Blend, out half Out, half Opacity)
{
    Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
    Out = lerp(Base, Out, Opacity);
}

void Unity_Distance_float3(float3 A, float3 B, out float Out)
{
    Out = distance(A, B);
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    half _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0 = Vector1_a906bcbc01034447a774e7157f439325;
    half _Float_86b90f585d9e419685c9d6129c32ad92_Out_0 = _Property_7d5ef2692d2f455e9b758d2241872a1f_Out_0;
    half2 _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_86b90f585d9e419685c9d6129c32ad92_Out_0.xx), half2 (0, 0), _TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3);
    half2 _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_87b83b503e624d4ba2e6223d1ee4d8e2_Out_3, _Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1);
    half _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_2;
    half _Float_34d1f47975c24b9d830b7e63338babb2_Out_0 = _Property_3b2fa75379e04f3a852ebb6643ceda3b_Out_0;
    half _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3;
    Unity_Rectangle_half(_Fraction_bc4ecc28f5194bd4acb0653f3d154f1a_Out_1, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Float_34d1f47975c24b9d830b7e63338babb2_Out_0, _Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3);
    half _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1;
    Unity_OneMinus_half(_Rectangle_294001f03a8844d0a31dcdd7fb17d4da_Out_3, _OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1);
    half _Property_816cd0f020e341689218fdfa135b61f0_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_1;
    half _Float_6a98aa8a551f43e09c01822dab195b73_Out_0 = _Property_816cd0f020e341689218fdfa135b61f0_Out_0;
    half2 _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3;
    Unity_TilingAndOffset_half(IN.uv0.xy, (_Float_6a98aa8a551f43e09c01822dab195b73_Out_0.xx), half2 (0, 0), _TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3);
    half2 _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1;
    Unity_Fraction_half2(_TilingAndOffset_6da8f99bc0274484b9770fe0926bba65_Out_3, _Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1);
    half _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0 = Vector1_a906bcbc01034447a774e7157f439325_3;
    half _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0 = _Property_029d48997b1b4d5e89aa95c4f228c444_Out_0;
    half _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3;
    Unity_Rectangle_half(_Fraction_d0da617e64d24b4fa08601d60d6ba55e_Out_1, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Float_eafbc5cfde864359a7121cb6bb207cea_Out_0, _Rectangle_3956398494ff4acea9b8910877d3c065_Out_3);
    half _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1;
    Unity_OneMinus_half(_Rectangle_3956398494ff4acea9b8910877d3c065_Out_3, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1);
    half _Blend_a56c5b0489e24745855fa904b20f4807_Out_2;
    Unity_Blend_Screen_half(_OneMinus_9b3c47487ce3429c957eab09bfb50bc9_Out_1, _OneMinus_d0a9a5dc69584a47b23e8e1395a4c549_Out_1, _Blend_a56c5b0489e24745855fa904b20f4807_Out_2, 1);
    float _Distance_c1f3965872c542578a1c3a4012301496_Out_2;
    Unity_Distance_float3(_WorldSpaceCameraPos, IN.WorldSpacePosition, _Distance_c1f3965872c542578a1c3a4012301496_Out_2);
    half2 _Property_c0f9d128ce18446ab3574833920ea240_Out_0 = Vector2_3bb87bf6a4674236a46e2b16cdeb72e9;
    float _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3;
    Unity_Remap_float(_Distance_c1f3965872c542578a1c3a4012301496_Out_2, _Property_c0f9d128ce18446ab3574833920ea240_Out_0, float2 (1, 0), _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3);
    float _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
    Unity_Multiply_float(_Blend_a56c5b0489e24745855fa904b20f4807_Out_2, _Remap_8a508cc69b7a434c9a4c8c24eb10f495_Out_3, _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2);
    surface.Alpha = _Multiply_f67e6f68e8324618815a6f3413d40cdf_Out_2;
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
    output.uv0 = input.texCoord0;
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
    }
        FallBack "Hidden/Shader Graph/FallbackError"
}