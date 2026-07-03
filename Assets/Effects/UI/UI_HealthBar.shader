Shader "UI/HealthBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Glow)]
        _GlowColor ("Glow Color", Color) = (1, 1, 0.5, 0.15)
        _GlowWidth ("Glow Width", Range(0, 0.3)) = 0.08

        [Header(Pulse)]
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 4
        _PulseIntensity ("Pulse Intensity", Range(0, 0.4)) = 0.15
        _PulseEnabled ("Pulse Enabled", Range(0, 1)) = 0

        [Header(Edge)]
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 0.4)

        [Header(UI Masking)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _GlowColor;
            float _GlowWidth;

            float _PulseSpeed;
            float _PulseIntensity;
            float _PulseEnabled;

            fixed4 _EdgeColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 texColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                fixed4 finalColor = texColor * IN.color;

                float pulse = 1.0;
                if (_PulseEnabled > 0.5 && _PulseSpeed > 0)
                {
                    pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseIntensity;
                    finalColor *= pulse;
                }

                float3 glow = _GlowWidth > 0
                    ? texColor.a * _GlowColor.rgb * _GlowColor.a
                    : 0;
                finalColor.rgb += glow;

                float edgeDist = min(IN.texcoord.x, 1.0 - IN.texcoord.x);
                edgeDist = min(edgeDist, min(IN.texcoord.y, 1.0 - IN.texcoord.y));
                float edgeMask = 1.0 - smoothstep(0, 0.02, edgeDist);
                finalColor.rgb = lerp(finalColor.rgb, _EdgeColor.rgb, edgeMask * _EdgeColor.a);

                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
