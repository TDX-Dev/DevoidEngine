#ifndef LIGHT_CONSTRUCTS
#define LIGHT_CONSTRUCTS

struct GPUPointLight
{
    float4 position; // xyz position, w = enabled
    float4 color; // rgb color, w = intensity
    float4 range; // x = radius, y = attenuationType, z = linear, w = quadratic
};

struct GPUSpotLight
{
    float4 position; // xyz position, w = enabled
    float4 color; // rgb color, w = intensity
    float4 direction; // xyz direction, w = range
    float innerCutoff;
    float outerCutoff;
    int shadowIndex;
    float padding;
};

struct GPUDirectionalLight
{
    float4 Direction; // xyz dir, w = enabled
    float4 Color; // rgb color, w = intensity
};

struct ShadowData
{
    float4x4 LightViewProj;
    float2 AtlasOffset;
    float2 AtlasScale;
    float3 LightPosition;
    float Padding;
};


#include "./MathConstants.hlsl"
#include "./PBRMethods.hlsl"
#include "./SH9.hlsl"


cbuffer SceneData : register(b3)
{
    uint pointLightCount;
    uint spotLightCount;
    uint directionalLightCount;
    uint _padding;
};

cbuffer ProbeGIData : register(b6)
{
    float3 ProbeGridMin;
    float ProbeSpacing;

    uint ProbeResolutionX;
    uint ProbeResolutionY;
    uint ProbeResolutionZ;
    uint ProbeCount;
};

StructuredBuffer<GPUPointLight> PointLights : register(t10);
StructuredBuffer<GPUSpotLight> SpotLights : register(t11);
StructuredBuffer<GPUDirectionalLight> DirectionalLights : register(t12);

StructuredBuffer<ShadowData> ShadowBuffer : register(t13);
StructuredBuffer<SH9> EnvironmentSH : register(t17);

StructuredBuffer<DiffuseProbe> DiffuseProbes : register(t20);

Texture2D ShadowAtlas : register(t9);
SamplerState ShadowSampler : register(s9);

TextureCube EnvironmentSkybox : register(t15);
TextureCube PrefilterMap : register(t16);
Texture2D BRDFLUT : register(t18);
Texture2D ScreenAO : register(t19);

SamplerState EnvironmentSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState ScreenAOSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

float ComputeShadow(int shadowIndex, float3 worldPos)
{
    ShadowData shadow = ShadowBuffer[shadowIndex];

    float4 lightSpace = mul(float4(worldPos, 1), shadow.LightViewProj);
    float3 proj = lightSpace.xyz / lightSpace.w;

    proj.xy = proj.xy * 0.5 + 0.5;
    proj.y = 1.0 - proj.y;

    if (proj.z > 1.0 || proj.z < 0.0)
        return 0.0;

    if (proj.x < 0.0 || proj.x > 1.0 || proj.y < 0.0 || proj.y > 1.0)
        return 0.0;

    proj.xy = shadow.AtlasOffset + proj.xy * shadow.AtlasScale;

    float dist = length(worldPos - shadow.LightPosition);

    float storedDist = ShadowAtlas.Sample(ShadowSampler, proj.xy).r;

    return dist > storedDist ? 1.0 : 0.0;
}

//float ComputeShadow(int shadowIndex, float3 worldPos)
//{
//    ShadowData shadow = ShadowBuffer[shadowIndex];

//    float4 lightSpace = mul(shadow.LightViewProj, float4(worldPos, 1));

//    float3 proj = lightSpace.xyz / lightSpace.w;

//    proj = proj * 0.5 + 0.5;

//    proj.xy = shadow.AtlasOffset + proj.xy * shadow.AtlasScale;

//    return frac(worldPos.x * 0.1);
//}


//float3 ComputeBRDF(
//    float3 N,
//    float3 V,
//    float3 L,
//    float3 albedo,
//    float metallic,
//    float roughness,
//    float3 F0,
//    float3 radiance
//)
//{
//    float3 H = normalize(V + L);

//    float NDF = DistributionGGX(N, H, roughness);
//    float G = GeometrySmith(N, V, L, roughness);
//    float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

//    float3 numerator = NDF * G * F;

//    float denom =
//        4.0 * max(dot(N, V), 0.0) *
//        max(dot(N, L), 0.0) + 0.0001;

//    float3 specular = numerator / denom;

//    float3 kS = F;
//    float3 kD = (1.0 - kS) * (1.0 - metallic);

//    float NdotL = max(dot(N, L), 0.0);

//    return (kD * albedo / PI + specular) * radiance * NdotL;
//}

float3 ComputeBRDF(
    float3 N,
    float3 V,
    float3 L,
    float3 albedo,
    float metallic,
    float roughness,
    float3 F0,
    float3 radiance,
    float clearcoat,
    float clearcoatRoughness)
{
    float NoV = saturate(dot(N, V));
    float NoL = saturate(dot(N, L));
   
    if (NoL <= 0.0) 
        return float3(0.0, 0.0, 0.0);

    float3 H = normalize(V + L);
    float NoH = saturate(dot(N, H));
    float VoH = saturate(dot(V, H));

    float D = DistributionGGX(NoH, roughness);
    float Vis = V_SmithGGXCorrelated(NoV, NoL, roughness);
    float3 F = FresnelSchlick(VoH, F0);
    float3 specular = D * Vis * F;
    float3 kD = (1.0 - F) * (1.0 - metallic);
    float3 diffuse = (kD * albedo) / PI;
    float coatD = DistributionGGX(NoH, clearcoatRoughness);
    float coatVis = V_SmithGGXCorrelated(NoV, NoL, clearcoatRoughness);
    float coatF = FresnelSchlick(VoH, float3(0.04, 0.04, 0.04)).r * clearcoat;
    
    float3 coatSpecular = coatD * coatVis * coatF;
    float3 baseLayer = (diffuse + specular) * (1.0 - coatF);

    return (baseLayer + coatSpecular) * radiance * NoL;
}

float ComputeAttenuation(float distanceSq, float range)
{
    float inverseRadius = 1.0 / range;

    float factor = distanceSq * inverseRadius * inverseRadius;

    float smoothFactor = max(1.0 - factor * factor, 0.0);

    return (smoothFactor * smoothFactor) / max(distanceSq, 0.0001);
}

float ComputeSpotAttenuation(float3 L, float3 lightDirection, float innerAngle, float outerAngle)
{
    float cosOuter = cos(outerAngle);

    float spotScale = 1.0 / max(cos(innerAngle) - cosOuter, 0.0001);

    float spotOffset = -cosOuter * spotScale;

    float cd = dot(-lightDirection, L);

    float attenuation = saturate(cd * spotScale + spotOffset);

    return attenuation * attenuation;
}


float3 ComputeDirectionalLight(
    GPUDirectionalLight light,
    float3 N,
    float3 V,
    float3 albedo,
    float metallic,
    float roughness,
    float3 F0,
    float clearcoat,
    float clearcoatRoughness
)
{
    float3 L = normalize(-light.Direction.xyz);

    float3 lightColor = light.Color.rgb * light.Color.w;

    float3 radiance = lightColor;

    return ComputeBRDF(
        N, V, L,
        albedo,
        metallic,
        roughness,
        F0,
        radiance,
        clearcoat,
        clearcoatRoughness
    );
}


float3 ComputePointLight(GPUPointLight light, float3 worldPos, float3 N, float3 V, float3 albedo, float metallic, float roughness, float3 F0, float clearcoat, float clearcoatRoughness)
{
    float3 toLight = light.position.xyz - worldPos;

    float distanceSq = dot(toLight, toLight);

    if (distanceSq <= 0.000001)
        return 0.0;

    float distance = sqrt(distanceSq);

    float range = light.range.x;

    if (distance > range)
        return 0.0;

    float3 L = toLight / distance;

    float attenuation = ComputeAttenuation(distanceSq, range);

    float3 radiance = light.color.rgb * light.color.w * attenuation;

    return ComputeBRDF(N, V, L, albedo, metallic, roughness, F0, radiance, clearcoat, clearcoatRoughness);
}

float3 ComputeSpotLight(GPUSpotLight light, float3 worldPos, float3 N, float3 V, float3 albedo, float metallic, float roughness, float3 F0, float clearcoat, float clearcoatRoughness)
{
    float3 toLight =
        light.position.xyz - worldPos;

    float distanceSq =
        dot(toLight, toLight);

    if (distanceSq <= 0.000001)
        return 0.0;

    float distance =
        sqrt(distanceSq);

    float range =
        light.direction.w;

    if (distance > range)
        return 0.0;

    float3 L =
        toLight / distance;

    float3 lightDirection =
        normalize(light.direction.xyz);

    float coneAttenuation =
        ComputeSpotAttenuation(
            L,
            lightDirection,
            light.innerCutoff,
            light.outerCutoff
        );

    if (coneAttenuation <= 0.0)
        return 0.0;

    float distanceAttenuation =
        ComputeAttenuation(
            distanceSq,
            range
        );

    float attenuation =
        coneAttenuation *
        distanceAttenuation;

    float3 radiance =
        light.color.rgb *
        light.color.w *
        attenuation;

    return ComputeBRDF(
        N, V, L,
        albedo,
        metallic,
        roughness,
        F0,
        radiance,
        clearcoat,
        clearcoatRoughness
    );
}

#endif