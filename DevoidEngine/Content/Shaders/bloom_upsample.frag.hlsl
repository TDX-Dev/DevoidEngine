struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

cbuffer BloomMipShaderData : register(b5)
{
    float2 mipSize;
    float filterRadius;
}

Texture2D INPUT_TEXTURE : register(t0);
Texture2D PREVIOUS_TEXTURE : register(t1);

SamplerState INPUT_TEXTURESampler : register(s0);

float3 Upsample(float2 uv, float2 pixelSize)
{
    const float2 coords[9] =
    {
        float2(-1.0, 1.0),
        float2(0.0, 1.0),
        float2(1.0, 1.0),

        float2(-1.0, 0.0),
        float2(0.0, 0.0),
        float2(1.0, 0.0),

        float2(-1.0, -1.0),
        float2(0.0, -1.0),
        float2(1.0, -1.0)
    };

    const float weights[9] =
    {
        0.0625, 0.125, 0.0625,
        0.125, 0.25, 0.125,
        0.0625, 0.125, 0.0625
    };

    float3 result = 0.0;

    [unroll]
    for (int i = 0; i < 9; i++)
    {
        float2 sampleUV = uv + coords[i] * pixelSize;

        result += weights[i] * PREVIOUS_TEXTURE.SampleLevel(INPUT_TEXTURESampler, sampleUV, 0).rgb;
    }

    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV;

    // Input is the CURRENT mip.
    float3 currentColor =
        INPUT_TEXTURE.SampleLevel(
            INPUT_TEXTURESampler,
            uv,
            0).rgb;

    // mipSize must be the PREVIOUS / smaller texture size.
    float2 previousPixelSize = 1.0 / mipSize;

    float3 previousColor = Upsample(uv, previousPixelSize);

    float3 result = lerp(
        currentColor,
        previousColor,
        filterRadius);

    return float4(result, 1.0);
}