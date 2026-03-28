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

//float4 PSMain(PSInput input) : SV_TARGET
//{
//    float2 uv = input.UV0;

//    float2 offset = mipSize * filterRadius;

//    float3 center = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv).rgb;

//    float3 s1 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(-offset.x, offset.y)).rgb;
//    float3 s2 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(offset.x, offset.y)).rgb;
//    float3 s3 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(-offset.x, -offset.y)).rgb;
//    float3 s4 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(offset.x, -offset.y)).rgb;

//    float3 result = center * 4.0;
//    result += (s1 + s2 + s3 + s4) * 2.0;
//    result *= 1.0 / 12.0;

//    return float4(result, 1.0);
//}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV0;

    float2 texelSize = 1.0 / mipSize;

    float radius = filterRadius * exp2(mipLevel);

    float2 offset = texelSize * radius;

    float3 center = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv).rgb;

    float3 s1 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(-offset.x, offset.y)).rgb;
    float3 s2 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(offset.x, offset.y)).rgb;
    float3 s3 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(-offset.x, -offset.y)).rgb;
    float3 s4 = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, uv + float2(offset.x, -offset.y)).rgb;

    float3 result = center * 4.0;
    result += (s1 + s2 + s3 + s4) * 2.0;
    result *= 1.0 / 12.0;

    return float4(result, 1.0);
}