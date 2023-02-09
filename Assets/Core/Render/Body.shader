Shader "Unlit/Body"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "./Body.hlsl"

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END

            BODY_INIT_PARAM(init_param)

            StructuredBuffer<Body> bodies;
            StructuredBuffer<BodyRender> renders;

            struct Attributes
            {
                uint id : SV_InstanceID;
                float4 pos : POSITION;
            };

            struct Varyings
            {
                uint id : SV_InstanceID;
                float4 pos : SV_POSITION;
            };

            Varyings vert(const Attributes a)
            {
                Varyings o;

                const uint id = a.id;
                o.id = id;

                float4 pos = a.pos;
                
                const Body body = bodies[id];
                const BodyRender render = renders[id];

                const float scale = body.size;
                const float3 p = render.pos;
                
                const float4x4 mat = {
                    scale, 0, 0, p.x,
                    0, scale, 0, p.y,
                    0, 0, scale, p.z,
                    0, 0, 0, 1,
                };
                pos = mul(mat, pos);
                
                o.pos = TransformObjectToHClip(pos.xyz);

                return o;
            }

            half4 frag(const Varyings v) : SV_Target
            {
                const BodyRender render = renders[v.id];
                return float4(render.color, 1);
            }
            ENDHLSL
        }
    }
}
