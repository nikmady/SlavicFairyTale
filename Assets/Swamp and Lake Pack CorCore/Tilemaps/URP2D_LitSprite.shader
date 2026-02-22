Shader "Custom/URP2D_LitSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "UniversalMaterialType"="2D"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float4 _Color;

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.uv = v.uv;
                o.color = v.color * _Color;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 albedo =
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

                half3 normalTS =
                    UnpackNormal(
                        SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv)
                    );

                return ApplyLighting2D(
                    albedo,
                    normalTS,
                    i.positionWS.xy
                );
            }

            ENDHLSL
        }
    }
}