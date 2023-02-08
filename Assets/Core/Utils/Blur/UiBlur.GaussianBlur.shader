Shader "Hidden/UiBlur/GaussianBlur"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    TEXTURE2D_X(_MainTexture);
    SAMPLER(sampler_MainTexture);
    uniform float4 _MainTexture_TexelSize;

    uniform float _spread;

    static const float4 gauss_weight[7] =
    {
        float4(0.0205, 0.0205, 0.0205, 0),
        float4(0.0855, 0.0855, 0.0855, 0),
        float4(0.232, 0.232, 0.232, 0),
        float4(0.324, 0.324, 0.324, 1),
        float4(0.232, 0.232, 0.232, 0),
        float4(0.0855, 0.0855, 0.0855, 0),
        float4(0.0205, 0.0205, 0.0205, 0)
    };

    //#region 降采样

    struct VaryingsDownSmpl
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord_left_top : TEXCOORD0;
        float2 texcoord_right_top : TEXCOORD1;
        float2 texcoord_left_bottom : TEXCOORD2;
        float2 texcoord_right_bottom : TEXCOORD3;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    VaryingsDownSmpl VertDownSmpl(Attributes input)
    {
        VaryingsDownSmpl output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        #if SHADER_API_GLES
        const float4 pos = input.positionOS;
        const float2 uv  = input.uv;
        #else
        const float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
        const float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
        #endif

        const float2 texcoord = uv;

        output.positionCS = pos;
        output.texcoord_left_top = texcoord + _MainTexture_TexelSize.xy * float2(-0.5, 0.5);
        output.texcoord_right_top = texcoord + _MainTexture_TexelSize.xy * float2(0.5, 0.5);
        output.texcoord_left_bottom = texcoord + _MainTexture_TexelSize.xy * float2(-0.5, -0.5);
        output.texcoord_right_bottom = texcoord + _MainTexture_TexelSize.xy * float2(0.5, -0.5);
        return output;
    }

    half4 FragDownSmpl(const VaryingsDownSmpl input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        const float4 color_left_top =
            SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, input.texcoord_left_top);
        const float4 color_left_bottom =
            SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, input.texcoord_left_bottom);
        const float4 color_right_top =
            SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, input.texcoord_right_top);
        const float4 color_right_bottom =
            SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, input.texcoord_right_bottom);

        const float4 color_sum = color_left_top + color_left_bottom + color_right_top + color_right_bottom;

        const float4 color = color_sum / 4;
        return color;
    }

    //#endregion

    //#region 模糊

    struct VaryingsBlur
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        float2 offset : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    VaryingsBlur VertBlurHorizontal(Attributes input)
    {
        VaryingsBlur output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        #if SHADER_API_GLES
        const float4 pos = input.positionOS;
        const float2 uv  = input.uv;
        #else
        const float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
        const float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
        #endif

        output.positionCS = pos;
        output.texcoord = uv;
        // 计算 X 方向的偏移量
        output.offset = float2(_MainTexture_TexelSize.x, 0) * _spread;
        return output;
    }

    VaryingsBlur VertBlurVertical(Attributes input)
    {
        VaryingsBlur output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        #if SHADER_API_GLES
        const float4 pos = input.positionOS;
        const float2 uv  = input.uv;
        #else
        const float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
        const float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
        #endif

        output.positionCS = pos;
        output.texcoord = uv;
        // 计算 Y 方向的偏移量
        output.offset = float2(0, _MainTexture_TexelSize.y) * _spread;
        return output;
    }

    half4 FragBlur(const VaryingsBlur input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        const float2 uv = input.texcoord;
        const float2 offset = input.offset;
        
        float2 offset_uv = uv - offset * 3;
        float4 color;
        
        for (int i = 0; i < 7; i ++)
        {
            const float4 col = SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, offset_uv);
            color += col * gauss_weight[i];
        
            offset_uv += offset;
        }
        
        return color;
    }

    //#endregion

    //#region 复制

    Varyings VertCopy(Attributes input)
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

    half4 FragCopy(const Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        const float4 color = SAMPLE_TEXTURE2D_X(_MainTexture, sampler_MainTexture, input.texcoord);

        return color;
    }

    //#endregion
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        // 降采样 pass
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDownSmpl
            #pragma fragment FragDownSmpl
            ENDHLSL
        }

        // 垂直方向模糊 pass
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertBlurHorizontal
            #pragma fragment FragBlur
            ENDHLSL
        }

        // 水平方向模糊 pass
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertBlurVertical
            #pragma fragment FragBlur
            ENDHLSL
        }

        // 复制 pass
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertCopy
            #pragma fragment FragCopy
            ENDHLSL
        }
    }
}