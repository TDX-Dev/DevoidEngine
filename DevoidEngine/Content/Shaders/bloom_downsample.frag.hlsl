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
SamplerState INPUT_TEXTURESampler : register(s0);


float3 Downsample(float2 uv, float2 pixelSize)
{
    const float2 coords[13] =
    {
        float2(-1.0, 1.0),
        float2(1.0, 1.0),
        float2(-1.0, -1.0),
        float2(1.0, -1.0),

        float2(-2.0, 2.0),
        float2(0.0, 2.0),
        float2(2.0, 2.0),

        float2(-2.0, 0.0),
        float2(0.0, 0.0),
        float2(2.0, 0.0),

        float2(-2.0, -2.0),
        float2(0.0, -2.0),
        float2(2.0, -2.0)
    };

    const float weights[13] =
    {
        0.125,
        0.125,
        0.125,
        0.125,

        0.05555555555,
        0.05555555555,
        0.05555555555,

        0.05555555555,
        0.05555555555,
        0.05555555555,

        0.05555555555,
        0.05555555555,
        0.05555555555
    };

    float3 result = 0.0;

    [unroll]
    for (int i = 0; i < 13; i++)
    {
        float2 sampleUV = uv + coords[i] * pixelSize;

        result += weights[i] * INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, sampleUV).rgb;
    }

    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{

    float2 pixelSize = (1.0 / mipSize) * 0.5;
    float3 result = Downsample(input.UV, pixelSize);
    return float4(result, 1.0);
}