Shader "DREAMS/Character Floor Shadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _ShadowColor ("Shadow tint", Color) = (0.18, 0.11, 0.14, 1)
        _Opacity ("Opacity", Range(0,1)) = 0.2
        _Contact ("Contact ellipse", Float) = 0
        _ContactCenter ("Contact center", Vector) = (0,0,0,0)
        _ContactRadii ("Contact radii", Vector) = (0.15,0.03,0,0)
        _FloorRect ("Floor min/max", Vector) = (-100,-1,100,0)
        _PixelsPerUnit ("Pixel grid", Float) = 100
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend DstColor Zero
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
                float4 _ShadowColor;
                float4 _ContactCenter;
                float4 _ContactRadii;
                float4 _FloorRect;
                float _Opacity;
                float _Contact;
                float _PixelsPerUnit;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.world = world.xy;
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                clip(input.world - _FloorRect.xy);
                clip(_FloorRect.zw - input.world);
                float mask;
                if (_Contact > 0.5)
                {
                    float2 pixel = floor(input.world * _PixelsPerUnit + 0.5) / _PixelsPerUnit;
                    float2 offset = (pixel - _ContactCenter.xy) / max(_ContactRadii.xy, 0.001);
                    mask = saturate((1 - dot(offset, offset)) * 2);
                }
                else mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(mask - 0.001);
                return half4(lerp(half3(1,1,1), _ShadowColor.rgb, saturate(mask * _Opacity)), 1);
            }
            ENDHLSL
        }
    }
}
