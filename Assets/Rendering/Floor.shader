Shader "Custom/NewUnlitUniversalRenderPipelineShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
        [Displacement] _Displacement("Displacement", Vector) = (0, 0, 0, 0)
        [Amplitude] _Amplitude("Amplitude", Float) = 0.5
        [Power] _Power("Power", Float) = 1.0
        [MaxColor] _MaxColor("MaxColor", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Background" "RenderPipeline" = "UniversalPipeline" }

        Pass
        { 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionOS : positionOS;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _Displacement;
                float _Amplitude;
                float _Power;
                float _MaxColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float x = IN.positionHCS.x - _Displacement.x;
                float y = IN.positionHCS.y - _Displacement.y;
                Float sqrDist = (x * x) + (y * y);
                color.g *= min( max( 1, 1 + pow(sqrDist + 1, -_Power)  * pow(_Amplitude, _Power) ), _MaxColor);
                color.r *= min(  max( 1, 1 + pow(sqrDist + 1, -_Power)  * pow(_Amplitude, _Power) ), _MaxColor);
                return color;
            }
            ENDHLSL
        }
    }
}
