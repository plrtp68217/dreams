Shader "DREAMS/Interaction Layer Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Layer Mask", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 0.88, 0.58, 0.95)
        _Width ("Width In Source Pixels", Range(1, 3)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="False" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _Width;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.color = input.color * unity_SpriteColor;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 stepUV = _MainTex_TexelSize.xy * round(_Width);
                half center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                half edge = 0;
                [unroll] for (int y = -1; y <= 1; y++)
                [unroll] for (int x = -1; x <= 1; x++)
                    edge = max(edge, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(x, y) * stepUV).a);
                half alpha = step(0.5h, edge) * (1 - step(0.5h, center));
                return half4(_OutlineColor.rgb * input.color.rgb, alpha * _OutlineColor.a * input.color.a);
            }
            ENDHLSL
        }
    }
}
