Shader "Custom/VideoSBS"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Scale ("Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 200

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Scale;

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv;
                
                // Center the UV coordinates
                uv = (uv - 0.5) * _Scale + 0.5;
                
                #ifdef UNITY_STEREO_INSTANCING_ENABLED
                    if (unity_StereoEyeIndex == 1)
                    {
                        // Right eye - use right half
                        uv.x = (uv.x + 1.0) * 0.5;
                    }
                    else
                    {
                        // Left eye - use left half
                        uv.x *= 0.5;
                    }
                #endif

                // Clamp UVs to prevent bleeding
                uv = saturate(uv);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                return col;
            }
            ENDHLSL
        }
    }
}
