Shader "Hidden/UiBlur/BoxBlur"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile scaling no_scaling

            TEXTURE2D_X(_MainTexture);
            SAMPLER(sampler_MainTexture);
            uniform float4 _MainTexture_TexelSize;

            int _quality;
            float _radius;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
                #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
                #endif

                output.positionCS = pos;
                output.texcoord = uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 color;

                #if scaling
                const float step_size_x = _MainTexture_TexelSize.x;
                const float step_size_y = _MainTexture_TexelSize.y;
                #else
                const float step_size_x = _MainTexture_TexelSize.x * _radius / _quality / 4.0;
                const float step_size_y = _MainTexture_TexelSize.y * _radius / _quality / 4.0;
                #endif

                for (int y = -_quality; y <= _quality; y++)
                {
                    for (int x = -_quality; x <= _quality; x++)
                    {
                        const float u = input.texcoord.x + x * step_size_x;
                        const float v = input.texcoord.y + y * step_size_y;
                        color += SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, float2(u, v));
                    }
                }
                const float mul = _quality * 2.0 + 1.0;
                color = color / (mul * mul);

                return color;
            }
            ENDHLSL
        }
    }
}