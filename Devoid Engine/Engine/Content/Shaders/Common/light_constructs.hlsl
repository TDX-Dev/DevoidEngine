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
    float2 padding;
};

struct GPUDirectionalLight
{
    float4 Direction; // xyz dir, w = enabled
    float4 Color; // rgb color, w = intensity
};

#include "../Common/math_constants.hlsl"
#include "../PBR/pbr_methods.hlsl"



cbuffer SceneData : register(b2)
{
    uint pointLightCount;
    uint spotLightCount;
    uint directionalLightCount;
    uint _padding;
};


StructuredBuffer<GPUPointLight> PointLights : register(t10);
StructuredBuffer<GPUDirectionalLight> DirectionalLights : register(t11);
StructuredBuffer<GPUSpotLight> SpotLights : register(t12);


float3 ComputeBRDF(
    float3 N,
    float3 V,
    float3 L,
    float3 albedo,
    float metallic,
    float roughness,
    float3 F0,
    float3 radiance
)
{
    float3 H = normalize(V + L);

    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

    float3 numerator = NDF * G * F;

    float denom =
        4.0 * max(dot(N, V), 0.0) *
        max(dot(N, L), 0.0) + 0.0001;

    float3 specular = numerator / denom;

    float3 kS = F;
    float3 kD = (1.0 - kS) * (1.0 - metallic);

    float NdotL = max(dot(N, L), 0.0);

    return (kD * albedo / PI + specular) * radiance * NdotL;
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
    float3 F0
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
        radiance
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
    float3 F0
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
        radiance
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
    float3 F0
)
{
    float3 toLight = light.position.xyz - worldPos;

    float distance = length(toLight);

    float3 L = normalize(toLight);

    float3 lightDir = normalize(-light.direction.xyz);

    float theta = dot(L, lightDir);

    float epsilon = light.innerCutoff - light.outerCutoff;

    float coneAtten = saturate((theta - light.outerCutoff) / epsilon);

    if (coneAtten <= 0)
        return 0;

    float attenuation = coneAtten;

    float3 lightColor = light.color.rgb * light.color.w;

    float3 radiance = lightColor * attenuation;

    return ComputeBRDF(
        N, V, L,
        albedo,
        metallic,
        roughness,
        F0,
        radiance
    );
}