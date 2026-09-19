struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

cbuffer HalationShaderData : register(b5)
{
    float intensity;
    float threshold;
    float2 padding;
    float3 color;
};

Texture2D SCENE_TEXTURE : register(t0);
SamplerState SCENE_TEXTURESampler : register(s0);

Texture2D BLURRED_TEXTURE : register(t1);
SamplerState BLURRED_TEXTURESampler : register(s1);

float Luminance(float3 value)
{
    return dot(
        value,
        float3(
            0.2126,
            0.7152,
            0.0722));
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV;

    float3 scene = SCENE_TEXTURE.Sample(
        SCENE_TEXTURESampler,
        uv).rgb;

    float3 blurred = BLURRED_TEXTURE.Sample(
        BLURRED_TEXTURESampler,
        uv).rgb;

    float3 bleed = max(
        blurred - scene,
        0.0);

    float blurredLuminance = Luminance(blurred);

    float mask = smoothstep(
        threshold,
        threshold + 1.0,
        blurredLuminance);

    bleed *= mask;
    bleed *= color;
    bleed *= intensity;

    return float4(
        bleed,
        1.0);
}