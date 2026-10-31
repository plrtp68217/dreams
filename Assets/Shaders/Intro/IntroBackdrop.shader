Shader "DREAMS/Intro/Backdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            fixed4 _Color;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv - 0.5;
                float vignette = saturate(1 - dot(p * float2(1.15, 1), p * float2(1.15, 1)) * 2);
                float grain = frac(sin(dot(floor(i.uv * float2(640, 360)), float2(12.9898,78.233))) * 43758.5453);
                float3 baseColor = float3(0.006, 0.008, 0.014) + vignette * float3(0.003, 0.004, 0.005);
                baseColor += (grain - 0.5) * 0.004;
                #ifndef UNITY_COLORSPACE_GAMMA
                baseColor = GammaToLinearSpace(baseColor);
                #endif
                return fixed4(baseColor, 1) * i.color;
            }
            ENDCG
        }
    }
}
