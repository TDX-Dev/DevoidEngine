struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz + handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 WorldspacePosition : TEXCOORD3;
};

TextureCube MAT_ENVIRONMENT_MAP : register(t0);
SamplerState MAT_ENVIRONMENT_MAP_SAMPLER : register(s0);

cbuffer Material : register(b3)
{
    float Roughness;
};

static const float PI = 3.14159265359;

#include "./skybox_constants.hlsl"

float DistributionGGX(float NdotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;

    float denom = (NdotH * NdotH) * (a2 - 1.0) + 1.0;
    denom = PI * denom * denom;

    return a2 / denom;
}

// --- Hammersley sequence ---
float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10;
}

float2 Hammersley(uint i, uint N)
{
    return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

// --- GGX importance sampling ---
float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness)
{
    float a = roughness * roughness;

    float phi = 2.0 * PI * Xi.x;
    float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
    float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;

    // build tangent basis
    float3 up = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
    float3 tangent = normalize(cross(up, N));
    float3 bitangent = cross(N, tangent);

    float3 sampleVec =
        tangent * H.x +
        bitangent * H.y +
        N * H.z;

    return normalize(sampleVec);
}

float4 PSMain(PSInput input) : SV_Target
{
    float3 N = normalize(input.WorldspacePosition);
    float3 R = N;
    float3 V = R;

    const uint SAMPLE_COUNT = 1024u;

    float3 prefilteredColor = float3(0, 0, 0);
    float totalWeight = 0.0;

    for (uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, N, Roughness);
        float3 L = normalize(2.0 * dot(V, H) * H - V);

        float NdotL = saturate(dot(N, L));
        if (NdotL > 0.0)
        {
            float NdotH = saturate(dot(N, H));
            float HdotV = max(dot(H, V), 0.001);

            float D = DistributionGGX(NdotH, Roughness);
            float pdf = (D * NdotH / (4.0 * HdotV)) + 0.0001;

            float saTexel = 4.0 * PI / (6.0 * ENV_RESOLUTION * ENV_RESOLUTION);
            float saSample = 1.0 / (float(SAMPLE_COUNT) * pdf + 0.0001);

            float mipLevel = Roughness == 0.0 ? 0.0 : 0.5 * log2(saSample / saTexel);

            mipLevel = clamp(mipLevel, 0.0, MAX_PREFILTER_MIP_LEVEL);

            L.y = -L.y;
            
            float3 color = MAT_ENVIRONMENT_MAP.SampleLevel(
                MAT_ENVIRONMENT_MAP_SAMPLER,
                L,
                mipLevel
            ).rgb;

            prefilteredColor += color * NdotL;
            totalWeight += NdotL;
        }
    }

    prefilteredColor /= max(totalWeight, 0.0001);

    return float4(prefilteredColor, 1.0);
}