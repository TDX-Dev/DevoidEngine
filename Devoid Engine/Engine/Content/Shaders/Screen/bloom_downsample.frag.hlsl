struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT;
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragPos : TEXCOORD2;
    float3 WorldPos : TEXCOORD3;
};

cbuffer BloomMipShaderData : register(b2)
{
    float2 mipSize;
    int mipLevel;
    float filterRadius;
}

Texture2D INPUT_TEXTURE : register(t0);
SamplerState INPUT_TEXTURESampler : register(s0);

float Luma(float3 c)
{
    return dot(c, float3(0.2126, 0.7152, 0.0722));
}

float KarisAverage(float3 col)
{
    float luma = Luma(col) * 0.25;
    return 1.0 / (1.0 + luma);
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 texCoord = input.UV0;
    float2 texel = 1.0 / mipSize;

    float x = texel.x;
    float y = texel.y;

    float3 a = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(-2 * x, 2 * y), 0).rgb;
    float3 b = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(0, 2 * y), 0).rgb;
    float3 c = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(2 * x, 2 * y), 0).rgb;

    float3 d = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(-2 * x, 0), 0).rgb;
    float3 e = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord, 0).rgb;
    float3 f = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(2 * x, 0), 0).rgb;

    float3 g = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(-2 * x, -2 * y), 0).rgb;
    float3 h = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(0, -2 * y), 0).rgb;
    float3 i = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(2 * x, -2 * y), 0).rgb;

    float3 j = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(-x, y), 0).rgb;
    float3 k = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(x, y), 0).rgb;
    float3 l = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(-x, -y), 0).rgb;
    float3 m = INPUT_TEXTURE.SampleLevel(INPUT_TEXTURESampler, texCoord + float2(x, -y), 0).rgb;

    float3 downsample;

    if (mipLevel == 0)
    {
        float3 g0 = (a + b + d + e) * (0.125 / 4.0);
        float3 g1 = (b + c + e + f) * (0.125 / 4.0);
        float3 g2 = (d + e + g + h) * (0.125 / 4.0);
        float3 g3 = (e + f + h + i) * (0.125 / 4.0);
        float3 g4 = (j + k + l + m) * (0.5 / 4.0);

        g0 *= KarisAverage(g0);
        g1 *= KarisAverage(g1);
        g2 *= KarisAverage(g2);
        g3 *= KarisAverage(g3);
        g4 *= KarisAverage(g4);

        downsample = g0 + g1 + g2 + g3 + g4;
        downsample = max(downsample, 0.0001);
    }
    else
    {
        downsample = e * 0.125;
        downsample += (a + c + g + i) * 0.03125;
        downsample += (b + d + f + h) * 0.0625;
        downsample += (j + k + l + m) * 0.125;
    }

    return float4(downsample, 1);
}