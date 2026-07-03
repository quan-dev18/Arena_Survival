Shader "Custom/OutlineShader"
{
    Properties
    {
        [HDR] [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0.0, 10.0)) = 1.0
        _OutlineOffset("Outline Offset", Vector) = (0, 0, 0, 0)
        [Toggle] _UseVertexColor("Use Vertex Color for Width", Float) = 0
        [Toggle] _OutlineOnly("Outline Only", Float) = 0

        [Header(Mask)]
        _OutlineMask("Outline Mask (Alpha Clip)", 2D) = "white" {}
        _MaskCutoff("Mask Cutoff", Range(0.0, 1.0)) = 0.5

        [Header(Advanced)]
        _DepthOffset("Depth Offset", Range(0.0, 1.0)) = 0.0
        [Toggle] _ZWrite("ZWrite", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 100

        // ---- Outline Pass ----
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZWrite Off
            ZTest LEqual
            ColorMask RGB

            HLSLPROGRAM
            #pragma vertex vert_outline
            #pragma fragment frag_outline
            #pragma multi_compile_instancing
            #pragma multi_compile _ _USEVERTEXCOLOR_ON
            #pragma multi_compile _ _OUTLINEONLY_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_OutlineMask);
            SAMPLER(sampler_OutlineMask);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _OutlineColor;
                float _OutlineWidth;
                float4 _OutlineOffset;
                float _MaskCutoff;
                float _DepthOffset;
            CBUFFER_END

            Varyings vert_outline(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float width = _OutlineWidth;

                #ifdef _USEVERTEXCOLOR_ON
                width *= IN.color.r;
                #endif

                float3 normalOS = normalize(IN.normalOS);
                float3 offsetOS = normalOS * width * 0.01;
                offsetOS += _OutlineOffset.xyz;

                float3 positionOS = IN.positionOS.xyz + offsetOS;
                float4 positionHCS = TransformObjectToHClip(positionOS);

                #if defined(UNITY_REVERSED_Z)
                    positionHCS.z += _DepthOffset * 0.0001;
                #else
                    positionHCS.z -= _DepthOffset * 0.0001;
                #endif

                OUT.positionHCS = positionHCS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag_outline(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half mask = SAMPLE_TEXTURE2D(_OutlineMask, sampler_OutlineMask, IN.uv).r;
                clip(mask - _MaskCutoff);

                return half4(_OutlineColor.rgb, _OutlineColor.a * _BaseColor.a);
            }
            ENDHLSL
        }

        // ---- Base Pass ----
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _OutlineOnly;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                #ifdef _OUTLINEONLY_ON
                discard;
                return 0;
                #else
                return color;
                #endif
            }
            ENDHLSL
        }

        // ---- Shadow Caster Pass ----
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_OutlineMask);
            SAMPLER(sampler_OutlineMask);

            CBUFFER_START(UnityPerMaterial)
                float _MaskCutoff;
            CBUFFER_END

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                half mask = SAMPLE_TEXTURE2D(_OutlineMask, sampler_OutlineMask, IN.uv).r;
                clip(mask - _MaskCutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/InternalErrorShader"
}
