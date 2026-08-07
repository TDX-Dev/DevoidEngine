struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

cbuffer BloomMipShaderData : register(b2)
{
    float2 mipSize;
    int mipLevel;
    float filterRadius;
}

Texture2D INPUT_TEXTURE : register(t0);
SamplerState INPUT_TEXTURESampler : register(s0);

float3 Prefilter(float3 color)
{
    float threshold = 1.0;
    float knee = 0.5; // 50% soft transition

    float brightness = max(color.r, max(color.g, color.b));

    // Quadratic soft knee
    float soft = brightness - threshold + knee;
    soft = clamp(soft, 0.0, 2.0 * knee);
    soft = soft * soft / (4.0 * knee + 1e-5);

    // Hard threshold
    float contribution = max(soft, brightness - threshold);

    contribution /= max(brightness, 1e-5);

    return color * contribution;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float3 color = INPUT_TEXTURE.Sample(INPUT_TEXTURESampler, input.UV).rgb;

    color = Prefilter(color);

    return float4(color, 1);
}