Shader "DREAMS/VFX/Window Leaves"
{
    Properties
    {
        _MainTex ("Leaf poses", 2D) = "white" {}
        _LeafShadow ("Foliage shadow", Color) = (0.1411765, 0.3490196, 0.2588235, 1)
        _LeafLight ("Foliage light", Color) = (0.1960784, 0.4549020, 0.3411765, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _LeafShadow;
                half4 _LeafLight;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 pose = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // The two source browns have distinct red/green differences in gamma and linear space.
                // Preserve their pixel shading while using the hall foliage's exact green palette.
                half lightPixel = step(0.05h, pose.r - pose.g);
                half3 leafColor = lerp(_LeafShadow.rgb, _LeafLight.rgb, lightPixel);
                return half4(leafColor * input.color.rgb, pose.a * input.color.a);
            }
            ENDHLSL
        }
    }
}
