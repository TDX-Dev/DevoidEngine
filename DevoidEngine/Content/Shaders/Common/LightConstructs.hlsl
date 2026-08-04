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
#include "./Constants.hlsl"


cbuffer SceneData : register(b2)
{
    uint pointLightCount;
    uint spotLightCount;
    uint directionalLightCount;
    uint _padding;
};

StructuredBuffer<GPUPointLight> PointLights : register(t10);
StructuredBuffer<GPUSpotLight> SpotLights : register(t11);
StructuredBuffer<GPUDirectionalLight> DirectionalLights : register(t12);

StructuredBuffer<ShadowData> ShadowBuffer : register(t13);
StructuredBuffer<SH9> EnvironmentSH : register(t17);

Texture2D ShadowAtlas : register(t9);
SamplerState ShadowSampler : register(s9);

TextureCube EnvironmentSkybox : register(t15);
TextureCube PrefilterMap : register(t16);
Texture2D BRDFLUT : register(t18);

SamplerState EnvironmentSampler : register(s10);

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
    
    // Early exit if light is behind the surface
    if (NoL <= 0.0) 
        return float3(0.0, 0.0, 0.0);

    // Calculate Half-vector and shared dot products
    float3 H = normalize(V + L);
    float NoH = saturate(dot(N, H));
    float VoH = saturate(dot(V, H));

    // ------------------------------------------------------------------------
    // 1. Base Specular Lobe (GGX)
    // ------------------------------------------------------------------------
    float D = DistributionGGX(NoH, roughness);
    float Vis = V_SmithGGXCorrelated(NoV, NoL, roughness);
    float3 F = FresnelSchlick(VoH, F0);

    // Notice: No division! V_SmithGGXCorrelated handles the denominator.
    float3 specular = D * Vis * F;

    // ------------------------------------------------------------------------
    // 2. Base Diffuse Lobe (Lambert)
    // ------------------------------------------------------------------------
    // Energy conservation: diffuse light is what didn't reflect as specular
    float3 kD = (1.0 - F) * (1.0 - metallic);
    float3 diffuse = (kD * albedo) / PI;

    // ------------------------------------------------------------------------
    // 3. Clearcoat Lobe (GGX)
    // ------------------------------------------------------------------------
    // Polyurethane typically has an IOR yielding F0 = 0.04
    float coatD = DistributionGGX(NoH, clearcoatRoughness);
    float coatVis = V_SmithGGXCorrelated(NoV, NoL, clearcoatRoughness);
    
    // Evaluate Clearcoat Fresnel at VoH, multiplied by clearcoat weight
    float coatF = FresnelSchlick(VoH, float3(0.04, 0.04, 0.04)).r * clearcoat;
    
    float3 coatSpecular = coatD * coatVis * coatF;

    // ------------------------------------------------------------------------
    // 4. Final Energy Conservation & Assembly
    // ------------------------------------------------------------------------
    // The clearcoat layer absorbs light before it hits the base layer
    float3 baseLayer = (diffuse + specular) * (1.0 - coatF);

    return (baseLayer + coatSpecular) * radiance * NoL;
}

float ComputeAttenuation(GPUPointLight light, float distance)
{
    float radius = light.range.x;

    if (distance > radius)
        return 0.0;

    float attenuationType = light.range.y;
    float linearD = light.range.z;
    float quadratic = light.range.w;

    if (attenuationType == 0) // Custom
        return 1.0 / (1.0 + linearD * distance + quadratic * distance * distance);
    else if (attenuationType == 1) // Constant
        return 1.0;
    else if (attenuationType == 2) // Linear
        return saturate(1.0 - distance / radius);
    else if (attenuationType == 3) // Quadratic
    {
        float norm = distance / radius;
        return saturate(1.0 - norm * norm);
    }

    return 1.0;
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


float3 ComputePointLight(
    GPUPointLight light,
    float3 worldPos,
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
    float3 toLight = light.position.xyz - worldPos;

    float distance = length(toLight);

    float attenuation = ComputeAttenuation(light, distance);

    if (attenuation <= 0)
        return 0;

    float3 L = normalize(toLight);

    float3 lightColor = light.color.rgb * light.color.w;

    float3 radiance = lightColor * attenuation;
    
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

float3 ComputeSpotLight(
    GPUSpotLight light,
    float3 worldPos,
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
    float3 toLight = light.position.xyz - worldPos;

    float distance = length(toLight);

    float range = light.direction.w;

    if (distance > range)
        return 0;

    float3 L = normalize(toLight);

    float3 lightDir = normalize(light.direction.xyz);

    float theta = dot(-L, lightDir);

    float cosInner = cos(light.innerCutoff);
    float cosOuter = cos(light.outerCutoff);

    float epsilon = cosInner - cosOuter;

    float coneAtten = saturate((theta - cosOuter) / epsilon);

    if (coneAtten <= 0)
        return 0;

    // distance falloff
    float norm = distance / range;
    float distanceAtten = saturate(1.0 - norm * norm);

    float attenuation = coneAtten * distanceAtten;

    float3 lightColor = light.color.rgb * light.color.w;

    float3 radiance = lightColor * attenuation;

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