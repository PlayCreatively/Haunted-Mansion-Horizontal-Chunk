Shader "Universal Render Pipeline/Custom/TransparentSelfOccludeURP"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,0.5)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline"
               "Queue"="Transparent"
               "RenderType"="Transparent" }

        ////////////////////////////////////////////////
        // PASS 0 : DEPTH PRE‑PASS (self‑occlusion mask)
        ////////////////////////////////////////////////
        Pass
        {
            Name "DepthPrepass"

            // Tells URP this is a depth‑only pass and nudges it *ahead* of normal transparents
            Tags { "LightMode"="DepthOnly"
                   "QueueOffset"="-50"   // render at ~2950 instead of 3000
                   "DisableBatching"="True" } // ensure offset isn’t lost to SRP batching

            Cull Off        // write both front & back faces so thin shells self‑occlude
            ZWrite On
            ColorMask 0     // no colour output

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float4 _Color;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv          = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _Color;
                clip(tex.a - 0.01); // discard fully‑transparent pixels so they don’t stamp depth
                return 0;           // depth only
            }
            ENDHLSL
        }

        ////////////////////////////////////////////////
        // PASS 1 : TRANSPARENT FORWARD BLEND
        ////////////////////////////////////////////////
        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float4 _Color;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv          = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _Color;
                return col;
            }
            ENDHLSL
        }
    }
}
