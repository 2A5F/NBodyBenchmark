Shader "Hidden/DualKawaseBlur"
{
    Properties
    {
        [KeywordEnum(Down, UP)]_SAMPLE("Sample", Float) = 1
        _Offset("Offset", Float) = 3
        _ViewSize("ViewSize", Vector) = (128, 128, 0, 0)
    }
    SubShader
    {
        HLSLINCLUDE
        #pragma use_dxc
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float2 _ViewSize;
            float _Offset;
        CBUFFER_END

        TEXTURE2D(_MainTex);

        struct vi
        {
            uint vertexID : VERTEXID_SEMANTIC;
        };

        struct v2f_down
        {
            float4 positionCS : SV_POSITION;
            float2 uv0: UV0;
            float2 uv1: UV1;
            float2 uv2: UV2;
            float2 uv3: UV3;
            float2 uv4: UV4;
        };

        struct v2f_up
        {
            float4 positionCS : SV_POSITION;
            float2 uv0: UV0;
            float2 uv1: UV1;
            float2 uv2: UV2;
            float2 uv3: UV3;
            float2 uv4: UV4;
            float2 uv5: UV5;
            float2 uv6: UV6;
            float2 uv7: UV7;
        };

        void full_screen_basic(vi i, out float4 positionCS, out float2 uv)
        {
            float sign = UNITY_UV_STARTS_AT_TOP ? -1 : 1;
            uv = float2(i.vertexID << 1 & 2, i.vertexID & 2);
            float2 pos = float2(uv * float2(-2, -2 * sign) + float2(1, 1 * sign));
            positionCS.xy = pos * float2(1, sign);
            positionCS.zw = float2(UNITY_RAW_FAR_CLIP_VALUE, 1.0);
        }

        v2f_down vert_down(vi i)
        {
            v2f_down o;
            o.positionCS = GetFullScreenTriangleVertexPosition(i.vertexID);
            float2 uv = GetFullScreenTriangleTexCoord(i.vertexID);

            float2 scale = _MainTex_TexelSize.xy * 0.5;
            // float2 halfPixel = 0.5 / (_ViewSize / 2.0);

            o.uv0 = uv;
            // o.uv1 = uv - halfPixel * _Offset;
            // o.uv2 = uv + halfPixel * _Offset;
            // o.uv3 = uv + float2(halfPixel.x, -halfPixel.y) * _Offset;
            // o.uv4 = uv - float2(halfPixel.x, -halfPixel.y) * _Offset;

            o.uv1 = uv - scale * float2(1 + _Offset, 1 + _Offset);
            o.uv2 = uv + scale * float2(1 + _Offset, 1 + _Offset);
            o.uv3 = uv - float2(scale.x, -scale.y) * float2(1 + _Offset, 1 + _Offset);
            o.uv4 = uv + float2(scale.x, -scale.y) * float2(1 + _Offset, 1 + _Offset);

            return o;
        }

        float4 frag_down(v2f_down v2f) : SV_TARGET
        {
            float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv0) * 4;
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv1);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv2);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv3);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv4);

            return c / 8;
        }

        v2f_up vert_up(vi i)
        {
            v2f_up o;
            o.positionCS = GetFullScreenTriangleVertexPosition(i.vertexID);
            float2 uv = GetFullScreenTriangleTexCoord(i.vertexID);

            float2 scale = _MainTex_TexelSize.xy * 0.5;
            // float2 halfPixel = 0.5 / (_ViewSize * 2.0);
            //
            // o.uv0 = uv + float2(-halfPixel.x * 2.0, 0.0) * _Offset;
            //
            // o.uv1 = uv + float2(-halfPixel.x, halfPixel.y) * _Offset;
            // o.uv2 = uv + float2(0.0, halfPixel.y * 2.0) * _Offset;
            // o.uv3 = uv + float2(halfPixel.x, halfPixel.y) * _Offset;
            // o.uv4 = uv + float2(halfPixel.x * 2.0, 0.0) * _Offset;
            // o.uv5 = uv + float2(halfPixel.x, -halfPixel.y) * _Offset;
            // o.uv6 = uv + float2(0.0, -halfPixel.y * 2.0) * _Offset;
            // o.uv7 = uv + float2(-halfPixel.x, -halfPixel.y) * _Offset;

            float2 offset = float2(1 + _Offset, 1 + _Offset);

            o.uv0 = uv + float2(-scale.x * 2, 0) * offset;
		    o.uv1 = uv + float2(-scale.x, scale.y) * offset;
		    o.uv2 = uv + float2(0, scale.y * 2) * offset;
		    o.uv3 = uv + scale * offset;
		    o.uv4 = uv + float2(scale.x * 2, 0) * offset;
		    o.uv5 = uv + float2(scale.x, -scale.y) * offset;
		    o.uv6 = uv + float2(0, -scale.y * 2) * offset;
		    o.uv7 = uv - scale * offset;

            return o;
        }

        float4 frag_up(v2f_up v2f) : SV_TARGET
        {
            float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv0);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv1) * 2.0;
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv2);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv3) * 2.0;
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv4);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv5) * 2.0;
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv6);
            c += SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, v2f.uv7) * 2.0;

            return c / 12;
        }
        ENDHLSL

        Pass
        {
            Name "Down"

            HLSLPROGRAM
            #pragma vertex vert_down
            #pragma fragment frag_down
            ENDHLSL
        }
        Pass
        {
            Name "Up"

            HLSLPROGRAM
            #pragma vertex vert_up
            #pragma fragment frag_up
            ENDHLSL
        }
    }
}