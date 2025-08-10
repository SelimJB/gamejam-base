Shader "Custom/WobbleURP"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _Offset("Z‑Offset", Float) = 0
        _NormalOffset("Normal Offset", Float) = 0
        _TimeInfluence("Time Influence", Float) = 0
        _YDiv("Ydiv", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline"
        }

        Offset [_Offset], 0
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Offset;
                float _NormalOffset;
                float _TimeInfluence;
                float _YDiv;
            CBUFFER_END

            struct ForwardAttributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ForwardVaryings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ForwardVaryings vert(ForwardAttributes IN)
            {
                ForwardVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 pos = IN.positionOS;
                float timeFactor = round(_Time.y * _TimeInfluence);

                float waveX = sin(pos.y * _YDiv + timeFactor) * _NormalOffset;
                float waveY = sin(pos.x * _YDiv + timeFactor) * _NormalOffset;
                float waveZ = sin(pos.y * _YDiv + timeFactor) * _NormalOffset;

                pos.x += waveX;
                pos.y += waveY;
                pos.z += waveZ;

                OUT.positionHCS = TransformObjectToHClip(pos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(ForwardVaryings IN) : SV_Target
            {
                return half4(_Color.rgb, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Offset;
                float _NormalOffset;
                float _TimeInfluence;
                float _YDiv;
            CBUFFER_END

            Varyings vertShadow(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 pos = IN.positionOS;
                float timeFactor = round(_Time.y * _TimeInfluence);
                float waveX = sin(pos.y * _YDiv + timeFactor) * _NormalOffset;
                float waveY = sin(pos.x * _YDiv + timeFactor) * _NormalOffset;
                float waveZ = sin(pos.y * _YDiv + timeFactor) * _NormalOffset;
                pos.x += waveX;
                pos.y += waveY;
                pos.z += waveZ;

                Attributes displaced = IN;
                displaced.positionOS = float4(pos, 1.0);
                OUT.positionCS = GetShadowPositionHClip(displaced);

                #ifdef _ALPHATEST_ON
        OUT.uv = IN.uv;
                #endif

                return OUT;
            }

            float4 fragShadow(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback "VertexLit"
}