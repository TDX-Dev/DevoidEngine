static const float PI = 3.14159265;

float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return a2 / max(denom, 0.000001);
}

static const float MEDIUMP_FLT_MAX = 65504.0;

float saturateMediump(float x)
{
    return min(x, MEDIUMP_FLT_MAX);
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

float V_SmithGGXCorrelated(float N, float V, float L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    
    float a = roughness;
    float a2 = a * a;

    float GGXV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
    float GGXL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);

    return 0.5 / max(GGXV + GGXL, 0.00001);
}

float V_SmithGGXCorrelated(float NoV, float NoL, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;

    float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
    float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);

    return 0.5 / max(GGXV + GGXL, 1e-5);
}

float V_SmithGGXCorrelatedFast(float NoV, float NoL, float roughness)
{
    float a2 = roughness * roughness;
    a2 *= a2;

    float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
    float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);

    return 0.5 / (GGXV + GGXL);
}

float V_SmithGGXCorrelatedFast(float N, float V, float L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    
    float a = roughness;
    float GGXV = NdotL * (NdotV * (1.0 - a) + a);
    float GGXL = NdotV * (NdotL * (1.0 - a) + a);
    return 0.5 / (GGXV + GGXL);
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