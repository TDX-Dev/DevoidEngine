struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

#include "./Common/SH9.hlsl"

StructuredBuffer<DiffuseProbe> PreviousBounce : register(t0);

cbuffer ProbeGIData : register(b6)
{
    float3 GridMin;
    float Spacing;

    uint ResolutionX;
    uint ResolutionY;
    uint ResolutionZ;
    uint ProbeCount;
}

uint GetProbeIndex(uint3 coord)
{
    return coord.x +
           coord.y * ResolutionX +
           coord.z * ResolutionX * ResolutionY;
}

DiffuseProbe LoadProbe(uint3 coord)
{
    return PreviousBounce[GetProbeIndex(coord)];
}

bool IsValidCoord(int3 coord)
{
    return coord.x >= 0 &&
           coord.y >= 0 &&
           coord.z >= 0 &&
           coord.x < (int) ResolutionX &&
           coord.y < (int) ResolutionY &&
           coord.z < (int) ResolutionZ;
}

void AccumulateProbe(
    int3 coord,
    float interpolationWeight,
    float3 normal,
    inout float3 irradiance,
    inout float totalWeight)
{
    if (!IsValidCoord(coord))
        return;

    DiffuseProbe probe =
        LoadProbe((uint3) coord);

    float weight =
        interpolationWeight * probe.Weight;

    if (weight <= 0.0f)
        return;

    irradiance +=
        EvaluateDiffuseProbe(
            probe,
            normal) *
        weight;

    totalWeight += weight;
}

float4 PSMain(PSInput input) : SV_Target0
{
    float3 normal =
        normalize(input.Normal);

    float3 gridPosition =
        (input.WorldspacePosition - GridMin) /
        Spacing -
        0.5f;

    int3 baseCoord =
        (int3) floor(gridPosition);

    float3 fraction =
        frac(gridPosition);

    float wx0 = 1.0f - fraction.x;
    float wx1 = fraction.x;

    float wy0 = 1.0f - fraction.y;
    float wy1 = fraction.y;

    float wz0 = 1.0f - fraction.z;
    float wz1 = fraction.z;

    float3 irradiance = 0.0f;
    float totalWeight = 0.0f;

    AccumulateProbe(baseCoord + int3(0, 0, 0), wx0 * wy0 * wz0, normal, irradiance, totalWeight);
    AccumulateProbe(baseCoord + int3(1, 0, 0), wx1 * wy0 * wz0, normal, irradiance, totalWeight);
    AccumulateProbe(baseCoord + int3(0, 1, 0), wx0 * wy1 * wz0, normal, irradiance, totalWeight);
    AccumulateProbe(baseCoord + int3(1, 1, 0), wx1 * wy1 * wz0, normal, irradiance, totalWeight);

    AccumulateProbe(baseCoord + int3(0, 0, 1), wx0 * wy0 * wz1, normal, irradiance, totalWeight);
    AccumulateProbe(baseCoord + int3(1, 0, 1), wx1 * wy0 * wz1, normal, irradiance, totalWeight);
    AccumulateProbe(baseCoord + int3(0, 1, 1), wx0 * wy1 * wz1, normal, irradiance, totalWeight);
    AccumulateProbe(baseCoord + int3(1, 1, 1), wx1 * wy1 * wz1, normal, irradiance, totalWeight);

    if (totalWeight > 0.0f)
        irradiance /= totalWeight;

    return float4(irradiance, 1.0f);
}

//float4 PSMain(PSInput input) : SV_Target0
//{
//    float3 gridPosition =
//        (input.WorldspacePosition - GridMin) /
//        Spacing -
//        0.5f;

//    int3 coord = (int3) round(gridPosition);

//    bool valid =
//        IsValidCoord(coord);

//    if (!valid)
//        return float4(1.0f, 0.0f, 0.0f, 1.0f);

//    float3 normalized =
//        (float3) coord /
//        float3(
//            max(ResolutionX - 1, 1),
//            max(ResolutionY - 1, 1),
//            max(ResolutionZ - 1, 1));

//    return float4(normalized, 1.0f);
//}