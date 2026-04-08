struct PSInput
{
    float4 Position : SV_POSITION;
    float3 WorldspacePosition : TEXCOORD0;
};

cbuffer VolumetricLightIndex : register(b3)
{
    int currentLightIndex;
};

#include "../Common/render_constants.hlsl"
#include "../Common/light_constructs.hlsl"


float3 EvaluateSpotLight(GPUSpotLight light, float3 pos)
{
    float3 toLight = light.position.xyz - pos;

    float distance = length(toLight);
    float range = light.direction.w;

    if (distance > range)
        return 0;

    float3 L = normalize(toLight);

    float3 lightDir = normalize(-light.direction.xyz);

    float theta = dot(L, lightDir);

    float cosInner = cos(light.innerCutoff);
    float cosOuter = cos(light.outerCutoff);

    float epsilon = cosInner - cosOuter;

    float coneAtten = saturate((theta - cosOuter) / epsilon);

    if (coneAtten <= 0)
        return 0;

    float norm = distance / range;
    float distanceAtten = saturate(1.0 - norm * norm);

    float attenuation = coneAtten * distanceAtten;

    float3 lightColor = light.color.rgb * light.color.w;

    return lightColor * attenuation;
}


bool ClipRayToSpotCone(
    float3 rayStart,
    float3 rayDir,
    GPUSpotLight light,
    out float tEnter,
    out float tExit)
{
    float3 lightPos = light.position.xyz;
    float3 axis = normalize(-light.direction.xyz);
    float range = light.direction.w;

    float cosOuter = cos(light.outerCutoff);

    float3 toStart = rayStart - lightPos;

    float dStart = dot(toStart, axis);
    float dDir = dot(rayDir, axis);

    // Intersect ray with front and back planes of the cone
    float t0 = (-dStart) / dDir;
    float t1 = (range - dStart) / dDir;

    if (t0 > t1)
    {
        float tmp = t0;
        t0 = t1;
        t1 = tmp;
    }

    tEnter = t0;
    tExit = t1;

    if (tExit <= 0)
        return false;

    return true;
}

float3 IntegrateVolumetric(float3 rayStart, float3 rayEnd, float2 posXY)
{
    float3 ray = rayEnd - rayStart;
    float rayLength = length(ray);

    if (rayLength < 0.0001)
        return 0;

    float3 rayDir = ray / rayLength;

    GPUSpotLight light = SpotLights[currentLightIndex];

    // Clamp ray to light range
    rayLength = min(rayLength, light.direction.w);

    const int STEPS = 12;

    float stepSize = rayLength / STEPS;

    float3 pos = rayStart;

    float noise = frac(sin(dot(posXY, float2(12.9898, 78.233))) * 43758.5453);
    pos += rayDir * stepSize * noise;

    float density = 0.08;

    float3 scattering = 0;

    for (int s = 0; s < STEPS; s++)
    {
        pos += rayDir * stepSize;

        float3 lightSample = EvaluateSpotLight(light, pos);

        scattering += lightSample * density * stepSize;
    }

    return scattering;
}


float4 PSMain(PSInput input) : SV_Target
{
    float3 rayStart = Position;
    float3 rayEnd = input.WorldspacePosition;

    float3 scattering =
        IntegrateVolumetric(rayStart, rayEnd, input.Position.xy);

    return float4(scattering, 1);
}