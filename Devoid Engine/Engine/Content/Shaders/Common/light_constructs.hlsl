// =====================================================
// Light Structures
// =====================================================

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


// =====================================================
// Scene Data (Light Counts)
// =====================================================

cbuffer SceneData : register(b2)
{
    uint pointLightCount;
    uint spotLightCount;
    uint directionalLightCount;
    uint _padding;
};


// =====================================================
// Buffers
// =====================================================

StructuredBuffer<GPUPointLight> PointLights : register(t10);
StructuredBuffer<GPUSpotLight> SpotLights : register(t11);
StructuredBuffer<GPUDirectionalLight> DirectionalLights : register(t12);


// =====================================================
// Attenuation
// =====================================================

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


// =====================================================
// Directional Light
// =====================================================

float3 ApplyDirectionalLight(
    GPUDirectionalLight light,
    float3 normal,
    float3 viewDir,
    float3 albedo,
    float shininess)
{
    if (light.Direction.w == 0)
        return 0;

    float3 lightDir = normalize(-light.Direction.xyz);
    float3 lightColor = light.Color.rgb * light.Color.w;

    float NdotL = saturate(dot(normal, lightDir));
    float3 diffuse = albedo * lightColor * NdotL;

    float3 halfway = normalize(lightDir + viewDir);
    float spec = pow(saturate(dot(normal, halfway)), shininess);
    float3 specular = lightColor * spec;

    return diffuse + specular;
}


// =====================================================
// Point Light
// =====================================================

float3 ApplyPointLight(
    GPUPointLight light,
    float3 worldPos,
    float3 normal,
    float3 viewDir,
    float3 albedo,
    float shininess)
{
    if (light.position.w == 0)
        return 0;

    float3 lightPos = light.position.xyz;
    float3 toLight = lightPos - worldPos;
    float distance = length(toLight);

    float attenuation = ComputeAttenuation(light, distance);
    if (attenuation <= 0)
        return 0;

    float3 lightDir = normalize(toLight);
    float3 lightColor = light.color.rgb * light.color.w;

    float NdotL = saturate(dot(normal, lightDir));
    float3 diffuse = albedo * lightColor * NdotL;

    float3 halfway = normalize(lightDir + viewDir);
    float spec = pow(saturate(dot(normal, halfway)), shininess);
    float3 specular = lightColor * spec;

    return (diffuse + specular) * attenuation;
}


// =====================================================
// Spot Light
// =====================================================

float3 ApplySpotLight(
    GPUSpotLight light,
    float3 worldPos,
    float3 normal,
    float3 viewDir,
    float3 albedo,
    float shininess)
{
    if (light.position.w == 0)
        return 0;

    float3 lightPos = light.position.xyz;
    float3 toLight = lightPos - worldPos;
    float distance = length(toLight);

    float range = light.direction.w;
    if (distance > range)
        return 0;

    float3 lightDir = normalize(toLight);
    float3 spotDir = normalize(-light.direction.xyz);

    float theta = dot(lightDir, spotDir);
    float epsilon = light.innerCutoff - light.outerCutoff;
    float spotFactor = saturate((theta - light.outerCutoff) / epsilon);

    float attenuation = saturate(1.0 - distance / range);

    float3 lightColor = light.color.rgb * light.color.w;

    float NdotL = saturate(dot(normal, lightDir));
    float3 diffuse = albedo * lightColor * NdotL;

    float3 halfway = normalize(lightDir + viewDir);
    float spec = pow(saturate(dot(normal, halfway)), shininess);
    float3 specular = lightColor * spec;

    return (diffuse + specular) * attenuation * spotFactor;
}


// =====================================================
// Combined Lighting (Now Using SceneData)
// =====================================================

float3 ComputeLighting(
    float3 worldPos,
    float3 normal,
    float3 viewDir,
    float3 albedo)
{
    float3 result = 0;
    float shininess = 32.0;

    // Directional
    for (uint di = 0; di < directionalLightCount; di++)
    {
        result += ApplyDirectionalLight(
            DirectionalLights[di],
            normal,
            viewDir,
            albedo,
            shininess);
    }

    // Point
    for (uint pi = 0; pi < pointLightCount; pi++)
    {
        result += ApplyPointLight(
            PointLights[pi],
            worldPos,
            normal,
            viewDir,
            albedo,
            shininess);
    }

    // Spot
    for (uint si = 0; si < spotLightCount; si++)
    {
        result += ApplySpotLight(
            SpotLights[si],
            worldPos,
            normal,
            viewDir,
            albedo,
            shininess);
    }

    return result;
}
