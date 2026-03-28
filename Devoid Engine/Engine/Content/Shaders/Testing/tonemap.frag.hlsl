struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragPos : TEXCOORD2;
    float3 WorldPos : TEXCOORD3;
};

cbuffer Material : register(b2)
{
    float4 color;
};

Texture2D MAT_SceneColor : register(t0);
Texture2D MAT_BloomColor : register(t1);

SamplerState MAT_SceneColorSampler : register(s0);
SamplerState MAT_BloomColorSampler : register(s1);

static const float3x3 ACESInputMat =
{
    { 0.59719, 0.35458, 0.04823 },
    { 0.07600, 0.90834, 0.01566 },
    { 0.02840, 0.13383, 0.83777 }
};

// ODT_SAT => XYZ => D60_2_D65 => sRGB
static const float3x3 ACESOutputMat =
{
    { 1.60475, -0.53108, -0.07367 },
    { -0.10208, 1.10813, -0.00605 },
    { -0.00327, -0.07276, 1.07602 }
};

float3 RRTAndODTFit(float3 v)
{
    float3 a = v * (v + 0.0245786f) - 0.000090537f;
    float3 b = v * (0.983729f * v + 0.4329510f) + 0.238081f;
    return a / b;
}

float3 ACESFitted(float3 color)
{
    color = mul(ACESInputMat, color);

    // Apply RRT and ODT
    color = RRTAndODTFit(color);

    color = mul(ACESOutputMat, color);

    // Clamp to [0, 1]
    color = saturate(color);

    return color;
}

float3 AgxLog(float3 x)
{
    return log2(max(x, 1e-6));
}

float3 AgxExp(float3 x)
{
    return exp2(x);
}

float3 AgxContrast(float3 x)
{
    return x * x * (3.0 - 2.0 * x);
}

float3 AgxLookPunchy(float3 color)
{
    float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
    return lerp(luma.xxx, color, 1.35);
}

float3 TonemapAgX(float3 color)
{
    color = log2(max(color, 1e-6));

    const float minEv = -12.47393;
    const float maxEv = 4.026069;

    color = (color - minEv) / (maxEv - minEv);
    color = saturate(color);

    color = color * color * (3.0 - 2.0 * color);

    return color;
}


float4 PSMain(PSInput input) : SV_TARGET
{
    float3 hdr = MAT_SceneColor.Sample(MAT_SceneColorSampler, input.UV0).rgb;
    float3 bloom = MAT_BloomColor.Sample(MAT_BloomColorSampler, input.UV0).rgb;

    float exposure = 1;
    hdr *= exposure;

    float bloomStrength = 0.04;
    hdr += bloom * bloomStrength;
    
    float3 color = ACESFitted(hdr);
    //color = pow(color, 1.0 / 2.2);

    //float3 color = hdr;
    color = pow(color, 1.0 / 2.2);
    //return float4(color, 1);
    
    return float4(color, 1.0);
}