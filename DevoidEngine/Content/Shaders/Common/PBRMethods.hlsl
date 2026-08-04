#ifndef PBR_METHODS
#define PBR_METHODS

#include "./MathConstants.hlsl"
#include "./Constants.hlsl"


float DistributionGGX(float NoH, float perceptualRoughness)
{
    float a = perceptualRoughness * perceptualRoughness; // alpha = r^2
    float a2 = a * a; // alpha^2 = r^4
    
    float NoH2 = NoH * NoH;
    float denom = (NoH2 * (a2 - 1.0) + 1.0);
    
    return a2 / max(PI * denom * denom, 1e-6);
}

static const float MEDIUMP_FLT_MAX = 65504.0;

float saturateMediump(float x)
{
    return min(x, MEDIUMP_FLT_MAX);
}

float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float2 Hammersley(uint i, uint N)
{
    return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

float3 HemisphereImportanceSampleDGGX(float2 u, float linearRoughness)
{
    float a = linearRoughness;
    float phi = 2.0 * PI * u.x;
    
    // Optimized (a+1)*(a-1) -> float accuracy optimization from Filament
    float cosTheta2 = (1.0 - u.y) / (1.0 + (a + 1.0) * ((a - 1.0) * u.y));
    float cosTheta = sqrt(cosTheta2);
    float sinTheta = sqrt(max(0.0, 1.0 - cosTheta2));

    return float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
}

// Height-correlated Smith GGX Visibility
float Visibility(float NoV, float NoL, float a)
{
    float a2 = a * a;
    float GGXL = NoV * sqrt((NoL - NoL * a2) * NoL + a2);
    float GGXV = NoL * sqrt((NoV - NoV * a2) * NoV + a2);
    return 0.5 / (GGXV + GGXL);
}

float3 ImportanceSampleGGX(float2 Xi, float3 N, float perceptualRoughness)
{
    float a = perceptualRoughness * perceptualRoughness; // alpha = r^2
    float a2 = a * a; // alpha^2 = r^4

    float phi = 2.0 * PI * Xi.x;
    float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a2 - 1.0) * Xi.y));
    float sinTheta = sqrt(max(0.0, 1.0 - cosTheta * cosTheta));

    // Polar to Cartesian in tangent space
    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;

    // Build orthonormal tangent basis from Normal
    float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(0.0, 1.0, 0.0);
    float3 tangent = normalize(cross(up, N));
    float3 bitangent = cross(N, tangent);

    // Transform sample to world space
    float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;

    return NdotV / (NdotV * (1.0 - k) + k);
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);

    float ggx1 = GeometrySchlickGGX(NdotV, roughness);
    float ggx2 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float V_SmithGGXCorrelated(float NoV, float NoL, float perceptualRoughness)
{
    float a = perceptualRoughness * perceptualRoughness; // alpha = r^2
    float a2 = a * a; // alpha^2 = r^4

    float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
    float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);

    return 0.5 / max(GGXV + GGXL, 1e-5);
}

float V_SmithGGXCorrelatedFast(float NoV, float NoL, float perceptualRoughness)
{
    float a = perceptualRoughness * perceptualRoughness; // alpha = r^2

    float GGXV = NoL * (NoV * (1.0 - a) + a);
    float GGXL = NoV * (NoL * (1.0 - a) + a);

    return 0.5 / max(GGXV + GGXL, 1e-5);
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    float f = pow(1.0 - cosTheta, 5.0);
    return f + F0 * (1.0 - f);
}

//float3 FresnelSchlick(float cosTheta, float3 F0)
//{
//    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
//}

float3 FresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    cosTheta = saturate(cosTheta);

    float r = 1.0 - roughness;
    float3 F90 = max(float3(r, r, r), F0);

    return F0 + (F90 - F0) * pow(1.0 - cosTheta, 5.0);
}

float2 IntegrateBRDF_Multiscatter(float NoV, float linearRoughness, uint numSamples)
{
    float2 r = float2(0.0, 0.0);
    float3 V = float3(sqrt(1.0 - NoV * NoV), 0.0, NoV);

    for (uint i = 0u; i < numSamples; ++i)
    {
        float2 u = Hammersley(i, numSamples);
        float3 H = HemisphereImportanceSampleDGGX(u, linearRoughness);
        float3 L = 2.0 * dot(V, H) * H - V;

        float VoH = saturate(dot(V, H));
        float NoL = saturate(L.z);
        float NoH = saturate(H.z);

        if (NoL > 0.0)
        {
            // Visibility term includes (VoH / NoH) and NoL factors
            float v = Visibility(NoV, NoL, linearRoughness) * NoL * (VoH / NoH);
            float Fc = pow(1.0 - VoH, 5.0);

            // Multi-scattering channel accumulation
            r.x += v * Fc;
            r.y += v;
            //r.x += v * (1.0 - Fc);
            //r.y += v * Fc;
        }
    }

    return r * (4.0 / float(numSamples));
}

float3 EvaluateIrradianceSH(
    StructuredBuffer<SH9> SH,
    float3 N)
{
    float x = N.x;
    float y = N.y;
    float z = N.z;

    float basis[9];

    basis[0] = 0.282095;

    basis[1] = 0.488603 * y;
    basis[2] = 0.488603 * z;
    basis[3] = 0.488603 * x;

    basis[4] = 1.092548 * x * y;
    basis[5] = 1.092548 * y * z;
    basis[6] = 0.315392 * (3.0 * z * z - 1.0);
    basis[7] = 1.092548 * x * z;
    basis[8] = 0.546274 * (x * x - y * y);

    float3 irradiance = 0;

    [unroll]
    for (uint i = 0; i < 9; i++)
    {
        irradiance += SH[0].C[i].rgb * basis[i];
    }

    return max(irradiance, 0);
}

#endif